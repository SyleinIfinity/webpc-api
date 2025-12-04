using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Models.Entities;
using WEBPC_API.Models.Interfaces;
using WEBPC_API.Repositories.Interfaces;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Services.Business
{
    public class GioHangService : IGioHangService
    {
        private readonly IGioHangRepository _repo;
        private readonly ISanPhamRepository _sanPhamRepo;

        public GioHangService(IGioHangRepository repo, ISanPhamRepository sanPhamRepo)
        {
            _repo = repo;
            _sanPhamRepo = sanPhamRepo;
        }

        // Helper: Lấy hoặc tạo giỏ hàng nếu chưa có
        private async Task<GioHang> GetOrCreateGioHang(int maKhachHang)
        {
            var gioHang = await _repo.GetByKhachHangIdAsync(maKhachHang);
            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    MaKhachHang = maKhachHang,
                    NgayCapNhat = DateTime.Now
                };
                gioHang = await _repo.CreateAsync(gioHang);
                // Khởi tạo list rỗng để tránh null reference
                gioHang.ChiTietGioHangs = new List<ChiTietGioHang>();
            }
            return gioHang;
        }

        public async Task<GioHangResponse> GetGioHangByKhachHangAsync(int maKhachHang)
        {
            var gioHang = await GetOrCreateGioHang(maKhachHang);
            return MapToResponse(gioHang);
        }

        public async Task<GioHangResponse> AddToCartAsync(AddToCartRequest request)
        {
            var gioHang = await GetOrCreateGioHang(request.MaKhachHang);

            // Kiểm tra sản phẩm tồn tại
            var sanPham = await _sanPhamRepo.GetByIdAsync(request.MaSanPham);
            if (sanPham == null) throw new KeyNotFoundException("Sản phẩm không tồn tại");

            // Kiểm tra item đã có trong giỏ chưa
            var existingItem = gioHang.ChiTietGioHangs.FirstOrDefault(x => x.MaSanPham == request.MaSanPham);

            if (existingItem != null)
            {
                // Logic: Cộng dồn số lượng
                existingItem.SoLuong += request.SoLuong;
                await _repo.AddOrUpdateItemAsync(existingItem);
            }
            else
            {
                var newItem = new ChiTietGioHang
                {
                    MaGioHang = gioHang.MaGioHang,
                    MaSanPham = request.MaSanPham,
                    SoLuong = request.SoLuong
                };
                await _repo.AddOrUpdateItemAsync(newItem);
            }

            // Update ngày cập nhật giỏ
            gioHang.NgayCapNhat = DateTime.Now;
            await _repo.SaveChangesAsync();

            // Trả về giỏ hàng mới nhất
            return await GetGioHangByKhachHangAsync(request.MaKhachHang);
        }

        public async Task<GioHangResponse> UpdateCartItemAsync(UpdateCartItemRequest request)
        {
            var gioHang = await GetOrCreateGioHang(request.MaKhachHang);
            var existingItem = gioHang.ChiTietGioHangs.FirstOrDefault(x => x.MaSanPham == request.MaSanPham);

            if (existingItem == null) throw new KeyNotFoundException("Sản phẩm không có trong giỏ hàng");

            // Cập nhật số lượng mới
            existingItem.SoLuong = request.SoLuongMoi;
            await _repo.AddOrUpdateItemAsync(existingItem);

            return await GetGioHangByKhachHangAsync(request.MaKhachHang);
        }

        public async Task<GioHangResponse> RemoveFromCartAsync(int maKhachHang, int maSanPham)
        {
            var gioHang = await GetOrCreateGioHang(maKhachHang);
            await _repo.RemoveItemAsync(gioHang.MaGioHang, maSanPham);

            return await GetGioHangByKhachHangAsync(maKhachHang);
        }

        public async Task<bool> ClearCartAsync(int maKhachHang)
        {
            var gioHang = await _repo.GetByKhachHangIdAsync(maKhachHang);
            if (gioHang != null)
            {
                await _repo.ClearCartAsync(gioHang.MaGioHang);
                return true;
            }
            return false;
        }

        // Helper Map Response
        private GioHangResponse MapToResponse(GioHang gh)
        {
            var response = new GioHangResponse
            {
                MaGioHang = gh.MaGioHang,
                MaKhachHang = gh.MaKhachHang,
                NgayCapNhat = gh.NgayCapNhat,
                ChiTiet = new List<ChiTietGioHangResponse>()
            };

            if (gh.ChiTietGioHangs != null)
            {
                foreach (var item in gh.ChiTietGioHangs)
                {
                    var sp = item.SanPham;
                    // Lấy ảnh đại diện hoặc ảnh đầu tiên
                    string imgUrl = sp?.HinhAnhs?.FirstOrDefault(h => h.LaAnhDaiDien)?.UrlHinhAnh
                                    ?? sp?.HinhAnhs?.FirstOrDefault()?.UrlHinhAnh
                                    ?? "no-image.png";

                    response.ChiTiet.Add(new ChiTietGioHangResponse
                    {
                        MaSanPham = item.MaSanPham,
                        TenSanPham = sp != null ? sp.TenSanPham : "Unknown",
                        HinhAnh = imgUrl,
                        DonGia = sp != null ? sp.GiaBan : 0,
                        GiaKhuyenMai = sp != null ? (sp.GiaKhuyenMai ?? 0) : 0, // Dùng nullable
                        SoLuong = item.SoLuong
                    });
                }
            }

            response.TongSoLuongSanPham = response.ChiTiet.Sum(x => x.SoLuong);
            response.TongTienHang = response.ChiTiet.Sum(x => x.ThanhTien);

            return response;
        }
    }
}