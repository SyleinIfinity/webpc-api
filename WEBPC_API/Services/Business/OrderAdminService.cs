using WEBPC_API.Models.Enums;
using WEBPC_API.Repositories.Interfaces;
using WEBPC_API.Services.Interfaces;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Models.Entities;

namespace WEBPC_API.Services.Business
{
    public class OrderAdminService : IOrderAdminService
    {
        private readonly IDonHangRepository _donHangRepo;
        private readonly INhatKyHoatDongRepository _logRepo;
        private readonly IMailService _mailService;
        private readonly IConfiguration _config;
        private readonly IKhachHangRepository _khachHangRepo;

        public OrderAdminService(
                    IDonHangRepository donHangRepo,
                    INhatKyHoatDongRepository logRepo,
                    IMailService mailService,
                    IConfiguration config,
                    IKhachHangRepository khachHangRepo
                )
        {
            _donHangRepo = donHangRepo;
            _logRepo = logRepo;
            _mailService = mailService;
            _config = config;
            _khachHangRepo = khachHangRepo;
        }

        // --- 1. LẤY DANH SÁCH ĐƠN HÀNG ---
        public async Task<List<DonHang>> GetAllOrdersAsync()
        {
            // Yêu cầu Repository phải có hàm GetAllAsync (đã làm ở bước trước)
            return await _donHangRepo.GetAllAsync();
        }

        // --- 2. DUYỆT ĐƠN HÀNG ---
        public async Task<OrderProcessResponse> ApproveOrderAsync(int orderId, int staffId)
        {
            var donHang = await _donHangRepo.GetByIdAsync(orderId);
            if (donHang == null) return new OrderProcessResponse { Success = false, Message = "Không tìm thấy đơn hàng." };

            // [SỬA]: Check ChoXacNhan
            if (donHang.trangThai != TrangThaiDonHang.ChoXacNhan.ToString())
            {
                return new OrderProcessResponse { Success = false, Message = $"Đơn hàng đang ở trạng thái {donHang.trangThai}, không thể duyệt." };
            }

            // [SỬA]: Update DangGiao
            donHang.trangThai = TrangThaiDonHang.DangGiao.ToString();
            donHang.maNhanVienDuyet = staffId;

            await _donHangRepo.UpdateAsync(donHang);
            await _logRepo.AddLogAsync(new NhatKyHoatDong
            {
                MaNhanVien = staffId,
                HanhDong = $"Duyệt đơn {donHang.maCodeDonHang}",
                ThoiGian = DateTime.Now
            });

            // Gửi mail báo khách (nếu cần)
            await GuiMailThongBaoAsync(donHang, "Đơn hàng của bạn đã được duyệt và đang được giao.");

            return new OrderProcessResponse { Success = true, Message = "Duyệt đơn hàng thành công." };
        }

        // --- 3. TỪ CHỐI / HỦY ĐƠN HÀNG ---
        public async Task<OrderProcessResponse> RejectOrderAsync(RejectOrderRequest request, int nhanVienId)
        {
            // Lấy ID từ Request DTO (Vì Controller đã gán vào rồi)
            int orderId = request.OrderId;

            var donHang = await _donHangRepo.GetByIdAsync(orderId);
            if (donHang == null) return new OrderProcessResponse { Success = false, Message = "Không tìm thấy đơn hàng." };

            // [SỬA]: Check HoanThanh hoặc Huy
            if (donHang.trangThai == TrangThaiDonHang.HoanThanh.ToString() ||
                donHang.trangThai == TrangThaiDonHang.Huy.ToString())
            {
                return new OrderProcessResponse { Success = false, Message = "Đơn hàng đã hoàn tất hoặc đã hủy." };
            }

            var giaoDich = await _donHangRepo.GetTransactionByOrderIdAsync(orderId);
            bool isPaid = false;

            // [SỬA]: Check Success và DaThanhToan
            if (giaoDich != null && giaoDich.trangThai == TrangThaiThanhToan.Success.ToString()) isPaid = true;
            if (donHang.trangThai == TrangThaiDonHang.DaThanhToan.ToString()) isPaid = true;

            string trangThaiMoi = "";
            string message = "";

            if (!isPaid)
            {
                trangThaiMoi = TrangThaiDonHang.Huy.ToString(); // [SỬA]
                message = "Đơn chưa thanh toán. Đã hủy thành công.";
            }
            else
            {
                trangThaiMoi = TrangThaiDonHang.ChoHoanTien.ToString(); // [SỬA]
                message = "Đơn đã thanh toán. Đã chuyển hồ sơ sang chờ hoàn tiền.";
            }

            donHang.trangThai = trangThaiMoi;
            donHang.maNhanVienDuyet = nhanVienId;
            // donHang.GhiChu = request.LyDoTuChoi; // Nếu có trường ghi chú

            await _donHangRepo.UpdateAsync(donHang);

            await _logRepo.AddLogAsync(new NhatKyHoatDong
            {
                MaNhanVien = nhanVienId,
                HanhDong = $"Hủy đơn {donHang.maCodeDonHang}. Lý do: {request.LyDoTuChoi}",
                ThoiGian = DateTime.Now
            });

            // Gửi mail cho Admin nếu cần hoàn tiền
            if (trangThaiMoi == "ChoHoanTien")
            {
                await GuiMailBaoAdminRefund(donHang, request.LyDoTuChoi, giaoDich?.soTien ?? 0);
            }
            else
            {
                // Gửi mail báo khách là đơn đã hủy
                await GuiMailThongBaoAsync(donHang, $"Đơn hàng đã bị hủy. Lý do: {request.LyDoTuChoi}");
            }

            return new OrderProcessResponse
            {
                Success = true,
                Message = message,
                MaDonHang = donHang.maDonHang,
                TrangThaiMoi = trangThaiMoi
            };
        }

        // --- 4. XÁC NHẬN HOÀN TIỀN (REFUND) ---
        public async Task<OrderProcessResponse> ConfirmRefundAsync(int orderId, int adminId)
        {
            var donHang = await _donHangRepo.GetByIdAsync(orderId);
            if (donHang == null) return new OrderProcessResponse { Success = false, Message = "Không tìm thấy đơn hàng." };

            // [SỬA]: Check ChoHoanTien
            if (donHang.trangThai != TrangThaiDonHang.ChoHoanTien.ToString())
            {
                return new OrderProcessResponse { Success = false, Message = "Đơn hàng không ở trạng thái chờ hoàn tiền." };
            }

            var giaoDich = await _donHangRepo.GetTransactionByOrderIdAsync(orderId);
            if (giaoDich != null)
            {
                giaoDich.trangThai = TrangThaiThanhToan.Refunded.ToString(); // [SỬA] -> Đánh dấu đã hoàn tiền
            }

            donHang.trangThai = TrangThaiDonHang.Huy.ToString(); // [SỬA] -> Về Hủy hoàn toàn
            donHang.maNhanVienDuyet = adminId;

            await _donHangRepo.UpdateAsync(donHang);

            await _logRepo.AddLogAsync(new NhatKyHoatDong
            {
                MaNhanVien = adminId,
                HanhDong = $"Xác nhận hoàn tiền đơn {donHang.maCodeDonHang}",
                ThoiGian = DateTime.Now
            });

            // Gửi mail báo khách tiền đã về
            await GuiMailThongBaoAsync(donHang, "Yêu cầu hoàn tiền của bạn đã được xử lý thành công.");

            return new OrderProcessResponse
            {
                Success = true,
                Message = "Đã xác nhận hoàn tiền thành công.",
                MaDonHang = donHang.maDonHang,
                TrangThaiMoi = "Huy"
            };
        }

        // --- CÁC HÀM PHỤ (HELPER) ---

        private async Task GuiMailThongBaoAsync(DonHang donHang, string noiDung)
        {
            try
            {
                var khach = await _khachHangRepo.GetByIdAsync(donHang.maKhachHang);
                if (khach != null && !string.IsNullOrEmpty(khach.Email))
                {
                    await _mailService.SendEmailAsync(khach.Email, $"Thông báo đơn hàng {donHang.maCodeDonHang}", noiDung);
                }
            }
            catch { }
        }

        private async Task GuiMailBaoAdminRefund(DonHang donHang, string lyDo, decimal soTien)
        {
            try
            {
                string adminEmail = _config["AdminEmail"];
                if (!string.IsNullOrEmpty(adminEmail))
                {
                    string content = $"Đơn {donHang.maCodeDonHang} cần hoàn {soTien:N0}đ. Lý do hủy: {lyDo}";
                    await _mailService.SendEmailAsync(adminEmail, "[URGENT] Yêu cầu hoàn tiền", content);
                }
            }
            catch { }
        }
    }
}