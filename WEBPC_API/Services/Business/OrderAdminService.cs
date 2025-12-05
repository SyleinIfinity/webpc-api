using WEBPC_API.Models.Enums;
using WEBPC_API.Repositories.Interfaces;
using WEBPC_API.Services.Interfaces;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;

namespace WEBPC_API.Services.Business
{
    public class OrderAdminService : IOrderAdminService
    {
        private readonly IDonHangRepository _donHangRepo;
        private readonly INhatKyHoatDongRepository _logRepo; // 1. Khai báo thêm Repository ghi log
        private readonly IMailService _mailService; // Service có sẵn của em
        private readonly IConfiguration _config;    // Để đọc appsettings
        private readonly IKhachHangRepository _khachHangRepo; // Để lấy mail khách



        // 2. Inject vào Constructor (Thêm tham số logRepo vào đây)
        public OrderAdminService(
                    IDonHangRepository donHangRepo,
                    INhatKyHoatDongRepository logRepo,
                    IMailService mailService,       // <--- Thêm
                    IConfiguration config,          // <--- Thêm
                    IKhachHangRepository khachHangRepo // <--- Thêm
                )
        {
            _donHangRepo = donHangRepo;
            _logRepo = logRepo;
            _mailService = mailService;
            _config = config;
            _khachHangRepo = khachHangRepo;
        }

        // --- HÀM 1: TỪ CHỐI ĐƠN HÀNG (Giữ nguyên logic cũ) ---
        public async Task<OrderProcessResponse> RejectOrderAsync(int orderId, RejectOrderRequest request, int nhanVienId)
        {
            var donHang = await _donHangRepo.GetByIdAsync(orderId);
            if (donHang == null) return new OrderProcessResponse { Success = false, Message = "Không tìm thấy đơn hàng." };

            // ... (Giữ nguyên logic kiểm tra trạng thái cũ) ...
            if (donHang.trangThai == TrangThaiDonHang.HoanThanh.ToString() ||
               donHang.trangThai == TrangThaiDonHang.Huy.ToString())
            {
                return new OrderProcessResponse { Success = false, Message = "Đơn hàng đã hoàn tất hoặc đã hủy." };
            }

            var giaoDich = await _donHangRepo.GetTransactionByOrderIdAsync(orderId);
            string trangThaiMoi = "";
            string message = "";

            bool isPaid = giaoDich != null && giaoDich.trangThai == TrangThaiThanhToan.Success.ToString();

            if (!isPaid)
            {
                trangThaiMoi = TrangThaiDonHang.Huy.ToString();
                message = "Đơn chưa thanh toán. Đã hủy thành công.";
            }
            else
            {
                trangThaiMoi = TrangThaiDonHang.ChoHoanTien.ToString();
                message = "Đơn đã thanh toán. Đã chuyển hồ sơ sang Admin chờ hoàn tiền.";
            }

            donHang.trangThai = trangThaiMoi;
            donHang.maNhanVienDuyet = nhanVienId;

            await _donHangRepo.UpdateAsync(donHang);
            await _donHangRepo.SaveChangesAsync();

            // ==========================================================
            // LOGIC GỬI MAIL CHO ADMIN (NẾU CẦN HOÀN TIỀN)
            // ==========================================================
            if (trangThaiMoi == TrangThaiDonHang.ChoHoanTien.ToString())
            {
                try
                {
                    string adminEmail = _config["AdminEmail"]; // Lấy email từ appsettings
                    if (!string.IsNullOrEmpty(adminEmail))
                    {
                        string subject = $"[KHẨN] Yêu cầu hoàn tiền - Đơn hàng {donHang.maCodeDonHang}";
                        string content = $@"
                            <h3>Hệ thống có đơn hàng cần hoàn tiền gấp!</h3>
                            <p><b>Mã đơn:</b> {donHang.maCodeDonHang}</p>
                            <p><b>Số tiền khách đã trả:</b> {giaoDich.soTien:N0} VNĐ</p>
                            <p><b>Lý do hủy:</b> {request.LyDoTuChoi}</p>
                            <p><b>Nhân viên xử lý:</b> ID {nhanVienId}</p>
                            <hr/>
                            <p>Vui lòng đăng nhập Admin để thực hiện hoàn tiền (Refund).</p>
                        ";

                        // Gọi MailService có sẵn của em
                        await _mailService.SendEmailAsync(adminEmail, subject, content);
                    }
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi mail nhưng không chặn quy trình chính
                    Console.WriteLine($"Lỗi gửi mail Admin: {ex.Message}");
                }
            }
            // ==========================================================

            return new OrderProcessResponse
            {
                Success = true,
                Message = message,
                MaDonHang = donHang.maDonHang,
                TrangThaiMoi = trangThaiMoi
            };
        }

        // --- HÀM 2: XÁC NHẬN HOÀN TIỀN (SỬA ĐOẠN NÀY) ---
        // --- HÀM 2: XÁC NHẬN HOÀN TIỀN ---
        public async Task<OrderProcessResponse> ConfirmRefundAsync(int orderId, int adminId)
        {
            var donHang = await _donHangRepo.GetByIdAsync(orderId);
            // ... (Giữ nguyên logic kiểm tra đơn hàng cũ) ...
            if (donHang == null) return new OrderProcessResponse { Success = false, Message = "Không tìm thấy đơn hàng." };

            if (donHang.trangThai != TrangThaiDonHang.ChoHoanTien.ToString())
            {
                return new OrderProcessResponse { Success = false, Message = "Đơn hàng không ở trạng thái chờ hoàn tiền." };
            }

            var giaoDich = await _donHangRepo.GetTransactionByOrderIdAsync(orderId);
            if (giaoDich == null) return new OrderProcessResponse { Success = false, Message = "Không tìm thấy giao dịch gốc." };


            // Xử lý hoàn tiền
            giaoDich.trangThai = TrangThaiThanhToan.Refunded.ToString();
            donHang.trangThai = TrangThaiDonHang.Huy.ToString();
            donHang.maNhanVienDuyet = adminId;

            await _donHangRepo.UpdateAsync(donHang);
            await _donHangRepo.SaveChangesAsync();

            // Ghi log hệ thống (Code cũ em đã làm)
            await _logRepo.AddLogAsync("REFUND_CONFIRM",
                $"Admin {adminId} hoàn tiền đơn {donHang.maCodeDonHang}. Số tiền: {giaoDich.soTien}", adminId);

            // ==========================================================
            // LOGIC GỬI MAIL THÔNG BÁO CHO KHÁCH HÀNG
            // ==========================================================
            try
            {
                // 1. Lấy thông tin khách hàng để có Email
                var khachHang = await _khachHangRepo.GetByIdAsync(donHang.maKhachHang);

                // (Nếu repository của em chưa có hàm GetByIdAsync, em có thể dùng _context.KhachHangs.Find... 
                // nhưng tốt nhất dùng Repo như Clean Architecture)

                if (khachHang != null && !string.IsNullOrEmpty(khachHang.Email))
                {
                    string subject = $"Xác nhận hoàn tiền thành công - Đơn hàng {donHang.maCodeDonHang}";
                    string content = $@"
                        <h3>Xin chào {khachHang.HoTen},</h3>
                        <p>Yêu cầu hoàn tiền cho đơn hàng <b>{donHang.maCodeDonHang}</b> đã được duyệt.</p>
                        <p>Số tiền: <b style='color:red'>{giaoDich.soTien:N0} VNĐ</b></p>
                        <p>Tiền sẽ được chuyển lại vào tài khoản của bạn trong 24h.</p>
                        <p>Cảm ơn bạn đã sử dụng dịch vụ WEBPC.</p>
                    ";

                    await _mailService.SendEmailAsync(khachHang.Email, subject, content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi mail Khách: {ex.Message}");
            }
            // ==========================================================

            return new OrderProcessResponse
            {
                Success = true,
                Message = "Đã hoàn tiền và gửi mail thông báo.",
                MaDonHang = donHang.maDonHang,
                TrangThaiMoi = TrangThaiDonHang.Huy.ToString()
            };
        }
    }
}