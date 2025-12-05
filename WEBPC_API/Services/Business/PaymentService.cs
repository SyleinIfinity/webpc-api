using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WEBPC_API.Data;
using WEBPC_API.Helpers;
using WEBPC_API.Models.DTOs.Casso;
using WEBPC_API.Models.Entities;
using WEBPC_API.Services.Interfaces;
using System.Text.RegularExpressions;

namespace WEBPC_API.Services.Business
{
    // Class nhỏ để đọc cấu hình Casso Settings
    public class CassoSettings
    {
        public string ApiKey { get; set; }
    }

    public class PaymentService : IPaymentService
    {
        private readonly DataContext _context;
        private readonly VietQrHelper _vietQrHelper;
        private readonly CassoSettings _cassoSettings;

        public PaymentService(DataContext context, VietQrHelper vietQrHelper, IOptions<CassoSettings> cassoSettings)
        {
            _context = context;
            _vietQrHelper = vietQrHelper;
            _cassoSettings = cassoSettings.Value;
        }

        // --- PHẦN 1: TẠO QR THANH TOÁN ---
        public async Task<VietQrResponse> CreatePaymentQr(int maDonHang)
        {
            // 1. Lấy thông tin đơn hàng
            var donHang = await _context.DonHang.FindAsync(maDonHang);
            if (donHang == null) throw new Exception("Đơn hàng không tồn tại.");

            // 2. Tạo nội dung chuyển khoản chuẩn: "DH" + [Mã Code Đơn Hàng]
            // Ví dụ: Đơn có mã CODE123 -> Nội dung CK: "DH CODE123"
            // Lưu ý: Nội dung nên NGẮN GỌN, KHÔNG DẤU để ngân hàng không bị lỗi
            string noiDungCK = $"DH {donHang.maCodeDonHang}";

            // 3. Kiểm tra xem đã có bản ghi giao dịch (Pending) chưa, nếu chưa thì tạo
            var giaoDich = await _context.GiaoDichThanhToan
                .FirstOrDefaultAsync(x => x.maDonHang == maDonHang && x.trangThai == "Pending");

            if (giaoDich == null)
            {
                giaoDich = new GiaoDichThanhToan
                {
                    maDonHang = maDonHang,
                    phuongThuc = "VietQR", // Ghi nhận là thanh toán qua VietQR
                    soTien = donHang.tongTien,
                    trangThai = "Pending", // Chờ thanh toán
                    ngayTao = DateTime.Now
                };
                _context.GiaoDichThanhToan.Add(giaoDich);
                await _context.SaveChangesAsync();
            }

            // 4. Gọi Helper để tạo ảnh QR
            return await _vietQrHelper.GenerateQrAsync((double)donHang.tongTien, noiDungCK);
        }

        // --- PHẦN 2: XỬ LÝ WEBHOOK (AUTO BANKING) ---
        public async Task ProcessCassoWebhook(CassoWebhookData webhookData, string secureToken)
        {
            // A. BẢO MẬT: Kiểm tra Token Casso
            if (secureToken != _cassoSettings.ApiKey)
            {
                throw new UnauthorizedAccessException("Casso Secure Token không hợp lệ!");
            }

            if (webhookData.data == null) return;

            // B. Duyệt qua từng giao dịch mà Casso gửi về
            foreach (var trans in webhookData.data)
            {
                // --- BƯỚC CẢI TIẾN: DÙNG REGEX TÁCH MÃ ---

                // Pattern này nghĩa là: Tìm chữ "DH" theo sau là khoảng trắng, rồi lấy cụm ký tự phía sau
                // Ví dụ: "NGUYEN VAN A CK DH CODE123" -> Nó sẽ bắt được "CODE123"
                // RegexOptions.IgnoreCase: Không phân biệt hoa thường (dh, DH đều nhận)
                var match = Regex.Match(trans.description, @"DH\s+([A-Za-z0-9_-]+)", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    // Lấy mã đơn hàng đã tách được (Group[1] là phần nằm trong ngoặc tròn)
                    string codeTimDuoc = match.Groups[1].Value;

                    // --- TRUY VẤN TRỰC TIẾP (KHÔNG QUÉT TOÀN BỘ DB) ---
                    // Chỉ tìm đúng 1 đơn hàng có mã code này và đang chờ xác nhận
                    var order = await _context.DonHang
                        .FirstOrDefaultAsync(d => d.maCodeDonHang == codeTimDuoc
                                               && d.trangThai == "ChoXacNhan");

                    // Nếu tìm thấy đơn hàng và số tiền chuyển >= tổng tiền
                    if (order != null && trans.amount >= order.tongTien)
                    {
                        // 1. Cập nhật trạng thái đơn hàng
                        order.trangThai = "DaThanhToan";
                        // Lưu ý: Nếu quy trình bên em là DaThanhToan -> Chờ Giao Hàng thì sửa lại enum tương ứng

                        // 2. Cập nhật hoặc tạo mới GiaoDichThanhToan
                        var gd = await _context.GiaoDichThanhToan
                            .FirstOrDefaultAsync(g => g.maDonHang == order.maDonHang);

                        if (gd != null)
                        {
                            gd.trangThai = "Success";
                            gd.maGiaoDichMomo = trans.tid; // Lưu mã tham chiếu ngân hàng
                            gd.ngayTao = DateTime.Now;
                            gd.soTien = trans.amount; // Lưu số tiền thực tế khách chuyển
                        }
                        else
                        {
                            var newGd = new GiaoDichThanhToan
                            {
                                maDonHang = order.maDonHang,
                                phuongThuc = "VietQR",
                                soTien = trans.amount,
                                trangThai = "Success",
                                maGiaoDichMomo = trans.tid,
                                ngayTao = DateTime.Now
                            };
                            _context.GiaoDichThanhToan.Add(newGd);
                        }

                        // Ghi log ra console để em dễ debug khi chạy server
                        Console.WriteLine($"[AUTO BANKING] Đã duyệt đơn: {order.maCodeDonHang} - Số tiền: {trans.amount}");
                    }
                }
            }

            // C. Lưu tất cả thay đổi vào Database một lần cuối
            await _context.SaveChangesAsync();
        }
    }
}