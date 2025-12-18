using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WEBPC_API.Data;
using WEBPC_API.Helpers;
using WEBPC_API.Models.DTOs.Casso;
using WEBPC_API.Models.Entities;
using WEBPC_API.Services.Interfaces;
using WEBPC_API.Models.Enums; // [PHẦN 1] Import Enum
using System.Text.RegularExpressions;
using System.Data; // [PHẦN 4] Import để dùng IsolationLevel

namespace WEBPC_API.Services.Business
{
    public class CassoSettings
    {
        public string ApiKey { get; set; }
        public string WebhookSecret { get; set; }
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
            var donHang = await _context.DonHang.FindAsync(maDonHang);
            if (donHang == null) throw new Exception("Đơn hàng không tồn tại.");

            // [PHẦN 1]: Dùng Enum kiểm tra trạng thái
            if (donHang.trangThai == TrangThaiDonHang.DaThanhToan.ToString() ||
                donHang.trangThai == TrangThaiDonHang.Huy.ToString() ||
                donHang.trangThai == TrangThaiDonHang.HoanThanh.ToString())
            {
                throw new Exception($"Đơn hàng đang ở trạng thái '{donHang.trangThai}', không thể tạo thanh toán mới."); 
            }

            string noiDungCK = $"DH {donHang.maCodeDonHang}";

            // [PHẦN 1]: Dùng Enum Pending
            var giaoDich = await _context.GiaoDichThanhToan
                .FirstOrDefaultAsync(x => x.maDonHang == maDonHang && x.trangThai == TrangThaiThanhToan.Pending.ToString());

            if (giaoDich == null)
            {
                giaoDich = new GiaoDichThanhToan
                {
                    maDonHang = maDonHang,
                    phuongThuc = "VietQR",
                    soTien = donHang.tongTien,
                    trangThai = TrangThaiThanhToan.Pending.ToString(),
                    ngayTao = DateTime.Now
                };
                _context.GiaoDichThanhToan.Add(giaoDich);
                await _context.SaveChangesAsync();
            }
            else
            {
                giaoDich.ngayTao = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return await _vietQrHelper.GenerateQrAsync((double)donHang.tongTien, noiDungCK);
        }

        // --- PHẦN 2 & 4: XỬ LÝ WEBHOOK (AUTO BANKING) CÓ CONCURRENCY ---
        public async Task ProcessCassoWebhook(CassoWebhookData webhookData, string secureToken)
        {
            if (secureToken != _cassoSettings.WebhookSecret)
            {
                throw new UnauthorizedAccessException("Webhook secret không hợp lệ");
            }


            if (webhookData.data == null) return;

            foreach (var trans in webhookData.data)
            {
                // [PHẦN 4 - CONCURRENCY STEP 1] IDEMPOTENCY CHECK
                // Kiểm tra ngay xem TID (Mã giao dịch ngân hàng) này đã được xử lý thành công chưa.
                // Nếu đã có trong DB với trạng thái Success -> Bỏ qua ngay lập tức.
                bool isProcessed = await _context.GiaoDichThanhToan
                    .AnyAsync(g => g.maGiaoDichMomo == trans.tid
                                && g.trangThai == TrangThaiThanhToan.Success.ToString());

                if (isProcessed)
                {
                    Console.WriteLine($"[WEBHOOK] Bỏ qua giao dịch {trans.tid} - Đã xử lý trước đó.");
                    continue; // Nhảy sang giao dịch tiếp theo trong danh sách
                }

                // Regex tìm mã đơn hàng
                if (string.IsNullOrWhiteSpace(trans.description))
                    continue;

                var match = Regex.Match(trans.description, @"DH\s+([A-Za-z0-9_-]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string codeTimDuoc = match.Groups[1].Value;

                    // [PHẦN 4 - CONCURRENCY STEP 2] DATABASE LOCKING
                    // Sử dụng IsolationLevel.Serializable để khóa chặt dữ liệu khi đang xử lý
                    // Đảm bảo không có luồng nào khác chen ngang sửa đổi đơn hàng này
                    using (var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable))
                    {
                        try
                        {
                            // Truy vấn lại đơn hàng TRONG transaction để đảm bảo dữ liệu mới nhất
                            var order = await _context.DonHang
                                .FirstOrDefaultAsync(d => d.maCodeDonHang == codeTimDuoc);

                            // Kiểm tra lại trạng thái một lần nữa trong vùng an toàn (Critical Section)
                            if (order == null ||
                                order.trangThai == TrangThaiDonHang.DaThanhToan.ToString() || // Đã thanh toán
                                order.trangThai == TrangThaiDonHang.Huy.ToString())          // Hoặc đã hủy
                            {
                                // Nếu đơn đã xong rồi thì thôi, không làm gì cả
                                await transaction.RollbackAsync();
                                continue;
                            }


                            // Kiểm tra số tiền
                            if (trans.amount >= order.tongTien && trans.amount <= order.tongTien + 1000)
                            {
                                // 1. Cập nhật trạng thái đơn hàng -> DaThanhToan (Enum)
                                order.trangThai = TrangThaiDonHang.DaThanhToan.ToString();

                                // 2. Cập nhật GiaoDichThanhToan
                                var gd = await _context.GiaoDichThanhToan
                                    .FirstOrDefaultAsync(g => g.maDonHang == order.maDonHang
                                                           && g.trangThai == TrangThaiThanhToan.Pending.ToString());

                                if (gd != null)
                                {
                                    gd.trangThai = TrangThaiThanhToan.Success.ToString();
                                    gd.maGiaoDichMomo = trans.tid; // Lưu TID để check Idempotency lần sau
                                    gd.soTien = trans.amount;
                                    gd.ngayTao = DateTime.Now;
                                }
                                else
                                {
                                    // Tạo mới nếu chưa có Pending
                                    var newGd = new GiaoDichThanhToan
                                    {
                                        maDonHang = order.maDonHang,
                                        phuongThuc = "VietQR",
                                        soTien = trans.amount,
                                        trangThai = TrangThaiThanhToan.Success.ToString(),
                                        maGiaoDichMomo = trans.tid, // Lưu TID
                                        ngayTao = DateTime.Now
                                    };
                                    _context.GiaoDichThanhToan.Add(newGd);
                                }

                                await _context.SaveChangesAsync();
                                await transaction.CommitAsync();

                                Console.WriteLine($"[AUTO BANKING SUCCESS] Đã duyệt đơn: {order.maCodeDonHang} - TID: {trans.tid}");
                            }
                            else
                            {
                                // Chuyển thiếu tiền -> Chỉ log warning, không update đơn
                                Console.WriteLine($"[AUTO BANKING WARNING] Đơn {order.maCodeDonHang} thiếu tiền. Cần: {order.tongTien}, Nhận: {trans.amount}");
                                await transaction.RollbackAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            Console.WriteLine($"[AUTO BANKING ERROR] Lỗi transaction: {ex.Message}");
                            // Không throw lỗi ra ngoài để tránh Casso retry liên tục khi gặp lỗi logic nội bộ
                            // Chỉ log lại để dev sửa
                        }
                    }
                }
            }
        }

        // --- PHẦN 3: KIỂM TRA TRẠNG THÁI ---
        public async Task<string> GetTransactionStatus(int maDonHang)
        {
            var donHang = await _context.DonHang.FindAsync(maDonHang);

            if (donHang != null && donHang.trangThai == TrangThaiDonHang.DaThanhToan.ToString())
            {
                return "PAID";
            }

            var giaoDichSuccess = await _context.GiaoDichThanhToan
                .AnyAsync(x => x.maDonHang == maDonHang && x.trangThai == TrangThaiThanhToan.Success.ToString());

            return giaoDichSuccess ? "PAID" : "PENDING";
        }
    }
}