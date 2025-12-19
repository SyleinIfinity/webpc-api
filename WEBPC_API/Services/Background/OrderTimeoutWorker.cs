using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Models.Enums;

namespace WEBPC_API.Services.Background
{
    public class OrderTimeoutWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderTimeoutWorker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Quét mỗi 1 phút
        private readonly double _timeoutMinutes = 15; // Hết hạn sau 15 phút

        public OrderTimeoutWorker(IServiceProvider serviceProvider, ILogger<OrderTimeoutWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                        // Cấu hình thời gian timeout (15 phút)
                        var timeoutThreshold = DateTime.Now.AddMinutes(-15);

                        // Lấy danh sách các đơn hàng CẦN HỦY
                        // CHỈ hủy đơn VietQR chưa thanh toán. KHÔNG ĐỤNG VÀO ĐƠN COD.
                        var expiredOrders = await context.DonHang // Giữ nguyên DonHang nếu DBSet tên là DonHang
                            .Where(dh =>
                                // --- SỬA TÊN BIẾN THÀNH CHỮ THƯỜNG ---
                                dh.phuongThucThanhToan == "VietQR" &&

                                // Điều kiện 2: Chưa thanh toán hoặc chờ xác nhận
                                (dh.trangThai == "ChoThanhToan" || dh.trangThai == "ChoXacNhan") &&

                                // Điều kiện 3: Đã quá hạn 15 phút
                                dh.ngayDat < timeoutThreshold)
                            .Include(d => d.ChiTietDonHangs) // Include để hoàn kho
                            .ToListAsync(stoppingToken);

                        if (expiredOrders.Any())
                        {
                            foreach (var order in expiredOrders)
                            {
                                // 1. Cập nhật trạng thái (SỬA THÀNH CHỮ THƯỜNG)
                                order.trangThai = "Huy";
                                _logger.LogInformation($"[AutoCancel] Hủy đơn hàng #{order.maDonHang} (VietQR timeout).");

                                // 2. Hoàn lại số lượng tồn kho
                                if (order.ChiTietDonHangs != null)
                                {
                                    foreach (var ct in order.ChiTietDonHangs)
                                    {
                                        // --- SỬA LỖI DATA CONTEXT: SanPhams (Số nhiều) ---
                                        // Và sửa thuộc tính maSanPham (chữ thường)
                                        var sanPham = await context.SanPhams.FindAsync(ct.maSanPham);
                                        if (sanPham != null)
                                        {
                                            sanPham.SoLuongTon += ct.soLuong; // Sửa soLuong thành chữ thường nếu cần
                                        }
                                    }
                                }
                            }

                            // Lưu thay đổi
                            await context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong quá trình quét đơn hàng hết hạn.");
                }

                // Chờ 1 phút rồi quét lại
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task ProcessExpiredOrdersAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                // Mốc thời gian: Hiện tại - 15 phút
                var thresholdTime = DateTime.Now.AddMinutes(-_timeoutMinutes);

                // 1. LẤY CÁC ĐƠN HÀNG "CÓ VẺ" HẾT HẠN (Dựa trên ngày đặt trước)
                // Chỉ lấy đơn chưa thanh toán (ChoXacNhan, ChoThanhToan) VÀ Ngày đặt < 15p trước
                var potentialExpiredOrders = await context.DonHang
                    .Include(d => d.ChiTietDonHangs) // Include để hoàn kho
                    .Where(d => (d.trangThai == TrangThaiDonHang.ChoXacNhan.ToString() ||
                                 d.trangThai == TrangThaiDonHang.ChoThanhToan.ToString())
                                && d.ngayDat < thresholdTime)
                    .ToListAsync(stoppingToken);

                if (potentialExpiredOrders.Any())
                {
                    foreach (var order in potentialExpiredOrders)
                    {
                        bool shouldCancel = true;

                        // 2. [LOGIC MỚI] KIỂM TRA GIAO DỊCH PENDING
                        // Tìm xem đơn này có giao dịch nào đang Pending không
                        var pendingTransaction = await context.GiaoDichThanhToan
                            .Where(g => g.maDonHang == order.maDonHang &&
                                        g.trangThai == TrangThaiThanhToan.Pending.ToString())
                            .OrderByDescending(g => g.ngayTao) // Lấy cái mới nhất
                            .FirstOrDefaultAsync(stoppingToken);

                        if (pendingTransaction != null)
                        {
                            // Nếu có giao dịch Pending, kiểm tra xem nó tạo bao lâu rồi?
                            if (pendingTransaction.ngayTao > thresholdTime)
                            {
                                // Giao dịch Pending này MỚI TẠO (trong vòng 15p trở lại đây)
                                // -> Có nghĩa là khách vừa mới bấm nút thanh toán xong.
                                // -> ĐỪNG HỦY VỘI. Hãy đợi thêm.
                                _logger.LogInformation($"Skip Cancel: Đơn {order.maCodeDonHang} vừa có giao dịch mới lúc {pendingTransaction.ngayTao}");
                                shouldCancel = false;
                            }
                            else
                            {
                                // Giao dịch Pending này cũng ĐÃ CŨ (quá 15p rồi mà chưa thấy tiền về)
                                // -> HỦY LUÔN, và đánh dấu giao dịch là Failed
                                pendingTransaction.trangThai = TrangThaiThanhToan.Failed.ToString();
                                pendingTransaction.noiDungLoi = "Auto Cancel: Transaction Timeout";
                            }
                        }

                        // 3. TIẾN HÀNH HỦY ĐƠN VÀ HOÀN KHO
                        if (shouldCancel)
                        {
                            _logger.LogInformation($"Auto Cancel: Hủy đơn {order.maCodeDonHang} do quá hạn thanh toán.");

                            order.trangThai = TrangThaiDonHang.Huy.ToString();

                            // Hoàn lại tồn kho
                            if (order.ChiTietDonHangs != null)
                            {
                                foreach (var item in order.ChiTietDonHangs)
                                {
                                    var product = await context.SanPhams.FindAsync(item.maSanPham);
                                    if (product != null)
                                    {
                                        product.SoLuongTon += item.soLuong;
                                        _logger.LogInformation($"Restock: Sản phẩm {product.MaSanPham} +{item.soLuong}");
                                    }
                                }
                            }
                        }
                    }

                    // Lưu tất cả thay đổi (Update trạng thái đơn, Trạng thái giao dịch, Tồn kho)
                    await context.SaveChangesAsync(stoppingToken);
                }
            }
        }
    }
}