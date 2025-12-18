using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Models.Entities;
using WEBPC_API.Models.Enums;
using WEBPC_API.Repositories.Interfaces;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Services.Business
{
    public class DonHangService : IDonHangService
    {
        private readonly IDonHangRepository _donHangRepo;
        private readonly IGioHangRepository _gioHangRepo;
        private readonly DataContext _context;

        public DonHangService(IDonHangRepository donHangRepo, IGioHangRepository gioHangRepo, DataContext context)
        {
            _donHangRepo = donHangRepo;
            _gioHangRepo = gioHangRepo;
            _context = context;
        }

        // --- 1. LOGIC TẠO ĐƠN HÀNG (ĐÃ FIX TÊN BIẾN VÀ LOGIC GIÁ) ---
        public async Task<DonHang> CreateOrderAsync(TaoDonHangRequest request)
        {
            // Validate dữ liệu đầu vào
            if (string.IsNullOrEmpty(request.PhuongThucThanhToan) ||
               (request.PhuongThucThanhToan != "COD" && request.PhuongThucThanhToan != "VietQR"))
            {
                throw new Exception("Phương thức thanh toán không hợp lệ (COD hoặc VietQR).");
            }

            if (request.SelectedCartItemIds == null || !request.SelectedCartItemIds.Any())
                throw new Exception("Vui lòng chọn ít nhất một sản phẩm để thanh toán.");

            // Lấy giỏ hàng từ DB (Repository đã Include SanPham)
            var gioHang = await _gioHangRepo.GetByKhachHangIdAsync(request.MaKhachHang);

            if (gioHang == null || gioHang.ChiTietGioHangs == null || !gioHang.ChiTietGioHangs.Any())
                throw new Exception("Giỏ hàng trống hoặc không tồn tại.");

            // [QUAN TRỌNG]: Lọc sản phẩm theo MaChiTietGioHang (ID dòng)
            var selectedItems = gioHang.ChiTietGioHangs
                .Where(item => request.SelectedCartItemIds.Contains(item.MaChiTietGioHang))
                .ToList();

            if (!selectedItems.Any())
                throw new Exception("Các sản phẩm bạn chọn không hợp lệ hoặc không có trong giỏ hàng.");

            // Bắt đầu Transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tạo mã đơn hàng ngẫu nhiên
                string maCode = "DH" + DateTime.Now.ToString("yyMMdd") + new Random().Next(1000, 9999);

                // Khởi tạo đối tượng DonHang (Sử dụng đúng tên thuộc tính camelCase như Entity)
                var donHang = new DonHang
                {
                    maCodeDonHang = maCode,
                    maKhachHang = request.MaKhachHang,
                    ngayDat = DateTime.Now,

                    // Logic Trạng thái
                    trangThai = request.PhuongThucThanhToan == "VietQR"
                                ? TrangThaiDonHang.ChoThanhToan.ToString()
                                : TrangThaiDonHang.ChoXacNhan.ToString(),

                    phuongThucThanhToan = request.PhuongThucThanhToan,
                    nguoiNhan = request.NguoiNhan,
                    soDienThoaiGiao = request.SoDienThoai,
                    diaChiGiaoHang = request.DiaChiGiaoHang,

                    // [LƯU Ý]: Entity DonHang không có cột GhiChu nên ta bỏ qua request.GhiChu

                    phiVanChuyen = 0,
                    ChiTietDonHangs = new List<ChiTietDonHang>()
                };

                decimal tongTienHang = 0;

                foreach (var itemCart in selectedItems)
                {
                    if (itemCart.SanPham == null)
                        throw new Exception($"Sản phẩm (ID: {itemCart.MaSanPham}) không tồn tại.");

                    // [TÍNH GIÁ]: Lấy giá ưu tiên khuyến mãi từ bảng SanPham
                    decimal donGiaThucTe = (itemCart.SanPham.GiaKhuyenMai.HasValue && itemCart.SanPham.GiaKhuyenMai.Value > 0)
                                           ? itemCart.SanPham.GiaKhuyenMai.Value
                                           : itemCart.SanPham.GiaBan;

                    // Tạo ChiTietDonHang (Sử dụng đúng tên thuộc tính camelCase)
                    var chiTiet = new ChiTietDonHang
                    {
                        maSanPham = itemCart.MaSanPham,

                        // [ĐÃ BỎ]: tenSanPham, hinhAnh (Vì Entity ChiTietDonHang không lưu)

                        soLuong = itemCart.SoLuong,
                        donGiaLucMua = donGiaThucTe, // Lưu giá bán tại thời điểm mua
                        thanhTien = itemCart.SoLuong * donGiaThucTe
                    };

                    donHang.ChiTietDonHangs.Add(chiTiet);
                    tongTienHang += chiTiet.thanhTien;
                }

                // Cập nhật tổng tiền cuối cùng
                donHang.tongTien = tongTienHang + donHang.phiVanChuyen;

                // Lưu vào Database
                await _donHangRepo.AddAsync(donHang);

                // Xóa các món đã mua khỏi giỏ hàng
                await _gioHangRepo.RemoveCartItemsAsync(gioHang.MaGioHang, request.SelectedCartItemIds);

                await transaction.CommitAsync();

                return donHang;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                var innerMessage = ex.InnerException?.Message ?? "";
                if (innerMessage.Contains("FK_") || innerMessage.Contains("REFERENCE"))
                    throw new Exception("Lỗi dữ liệu ràng buộc (Khóa ngoại). Vui lòng kiểm tra lại.");

                throw new Exception("Đặt hàng thất bại: " + ex.Message);
            }
        }

        // --- 2. GET ALL ORDERS ---
        public async Task<List<DonHangResponse>> GetAllOrdersAsync()
        {
            var orders = await _donHangRepo.GetAllOrdersFullAsync();
            // Map Entity -> DTO Response
            return orders.Select(MapToResponse).ToList();
        }

        // --- 3. GET ORDER BY ID ---
        public async Task<DonHangResponse?> GetOrderByIdAsync(int id)
        {
            var order = await _donHangRepo.GetOrderByIdFullAsync(id);
            if (order == null) return null;
            return MapToResponse(order);
        }

        // --- 4. GET ORDERS BY CUSTOMER ---
        public async Task<List<DonHangResponse>> GetOrdersByCustomerAsync(int maKhachHang)
        {
            var orders = await _donHangRepo.GetOrdersByCustomerIdAsync(maKhachHang);
            return orders.Select(MapToResponse).ToList();
        }

        // --- HELPER MAPPING ---
        private DonHangResponse MapToResponse(DonHang donHang)
        {
            return new DonHangResponse
            {
                MaDonHang = donHang.maDonHang,
                MaCodeDonHang = donHang.maCodeDonHang,
                MaKhachHang = donHang.maKhachHang,
                NgayDat = donHang.ngayDat,
                TrangThai = donHang.trangThai,

                // [MỚI] Map phương thức thanh toán sang Response
                PhuongThucThanhToan = donHang.phuongThucThanhToan,

                NguoiNhan = donHang.nguoiNhan,
                SoDienThoaiGiao = donHang.soDienThoaiGiao,
                DiaChiGiaoHang = donHang.diaChiGiaoHang,
                PhiVanChuyen = donHang.phiVanChuyen,
                TongTien = donHang.tongTien,
                ChiTiet = donHang.ChiTietDonHangs?.Select(ct => new ChiTietDonHangResponse
                {
                    MaSanPham = ct.maSanPham,
                    TenSanPham = ct.SanPham?.TenSanPham ?? "Sản phẩm đã xóa",
                    HinhAnh = ct.SanPham?.HinhAnhs?.FirstOrDefault(h => h.LaAnhDaiDien)?.UrlHinhAnh
                              ?? ct.SanPham?.HinhAnhs?.FirstOrDefault()?.UrlHinhAnh
                              ?? "/images/no-image.png",
                    SoLuong = ct.soLuong,
                    DonGiaLucMua = ct.donGiaLucMua,
                    ThanhTien = ct.thanhTien
                }).ToList() ?? new List<ChiTietDonHangResponse>()
            };
        }

        public async Task CancelOrderAsync(int orderId, CancelOrderRequest request)
        {
            // 1. Lấy đơn hàng kèm chi tiết (để trả kho)
            var donHang = await _context.DonHang
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.maDonHang == orderId);

            if (donHang == null) throw new Exception("Đơn hàng không tồn tại.");

            // 2. [BẢO MẬT] Kiểm tra chủ sở hữu
            if (donHang.maKhachHang != request.MaKhachHang)
            {
                throw new Exception("Bạn không có quyền hủy đơn hàng này.");
            }

            // 3. Kiểm tra trạng thái có được phép hủy không?
            // Chỉ cho hủy khi: Chờ xác nhận (COD) HOẶC Chờ thanh toán (QR chưa trả tiền)
            if (donHang.trangThai != TrangThaiDonHang.ChoXacNhan.ToString() &&
                donHang.trangThai != TrangThaiDonHang.ChoThanhToan.ToString())
            {
                // Nếu đã thanh toán hoặc đang giao -> Không cho hủy
                throw new Exception($"Đơn hàng đang ở trạng thái '{donHang.trangThai}', không thể hủy. Vui lòng liên hệ CSKH.");
            }

            // 4. Thực hiện Hủy
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // A. Đổi trạng thái đơn
                    donHang.trangThai = TrangThaiDonHang.Huy.ToString();

                    // (Tùy chọn) Có thể lưu Lý do hủy vào một bảng Log hoặc Note (nếu có)
                    // Ở đây mình tạm thời chưa lưu lý do vào DB vì bảng DonHang chưa có cột Note

                    // B. Xử lý Giao dịch thanh toán (Nếu đang Pending -> Failed)
                    var pendingTrans = await _context.GiaoDichThanhToan
                        .FirstOrDefaultAsync(g => g.maDonHang == orderId &&
                                                  g.trangThai == TrangThaiThanhToan.Pending.ToString());

                    if (pendingTrans != null)
                    {
                        pendingTrans.trangThai = TrangThaiThanhToan.Failed.ToString();
                        pendingTrans.noiDungLoi = $"User Cancel: {request.LyDoHuy}";
                    }

                    // C. [QUAN TRỌNG] Hoàn lại số lượng Tồn Kho
                    if (donHang.ChiTietDonHangs != null)
                    {
                        foreach (var item in donHang.ChiTietDonHangs)
                        {
                            // Lưu ý: Dùng _context.SanPhams (hoặc SanPham tùy DBContext của bạn)
                            // Dựa trên các file trước, context của bạn có thể là SanPham (số ít) hoặc SanPhams (số nhiều)
                            // Hãy check lại DataContext.cs. Theo file bạn gửi là 'SanPhams' (số nhiều)
                            var product = await _context.SanPhams.FindAsync(item.maSanPham);

                            if (product != null)
                            {
                                product.SoLuongTon += item.soLuong;
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}