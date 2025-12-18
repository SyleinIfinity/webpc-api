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
        // Tìm hàm MapToResponse và sửa lại phần mapping ChiTiet
        private GioHangResponse MapToResponse(GioHang gioHang)
        {
            if (gioHang == null) return null;

            // Tính lại tổng tiền
            decimal tongTien = 0;
            int tongSoLuong = 0;

            var listChiTiet = new List<ChiTietGioHangResponse>();

            if (gioHang.ChiTietGioHangs != null)
            {
                foreach (var item in gioHang.ChiTietGioHangs)
                {
                    if (item.SanPham == null) continue;

                    // Lấy ảnh đại diện
                    string hinhAnh = "";
                    if (item.SanPham.HinhAnhs != null && item.SanPham.HinhAnhs.Any())
                    {
                        var anhDaiDien = item.SanPham.HinhAnhs.FirstOrDefault(x => x.LaAnhDaiDien);
                        hinhAnh = anhDaiDien != null ? anhDaiDien.UrlHinhAnh : item.SanPham.HinhAnhs.First().UrlHinhAnh;
                    }

                    // Tính giá
                    decimal giaHienTai = item.SanPham.GiaKhuyenMai ?? item.SanPham.GiaBan;
                    decimal thanhTienItem = giaHienTai * item.SoLuong;

                    tongTien += thanhTienItem;
                    tongSoLuong += item.SoLuong;

                    listChiTiet.Add(new ChiTietGioHangResponse
                    {
                        // [MỚI] Map MaChiTietGioHang từ Entity sang Response
                        MaChiTietGioHang = item.MaChiTietGioHang,

                        MaSanPham = item.MaSanPham,
                        TenSanPham = item.SanPham.TenSanPham,
                        HinhAnh = hinhAnh,
                        DonGia = item.SanPham.GiaBan,
                        GiaKhuyenMai = item.SanPham.GiaKhuyenMai ?? 0,
                        SoLuong = item.SoLuong,
                        ThanhTien = thanhTienItem
                    });
                }
            }

            return new GioHangResponse
            {
                MaGioHang = gioHang.MaGioHang,
                MaKhachHang = gioHang.MaKhachHang,
                NgayCapNhat = gioHang.NgayCapNhat,
                TongTienHang = tongTien,
                TongSoLuongSanPham = tongSoLuong,
                ChiTietGioHangs = listChiTiet
            };
        }

        public async Task<GioHangResponse> GetSelectedItemsDetailAsync(GetSelectedCartItemsRequest request)
        {
            // 1. Lấy thông tin giỏ hàng cơ bản để có MaGioHang
            var gioHang = await _repo.GetByKhachHangIdAsync(request.MaKhachHang);
            if (gioHang == null)
            {
                throw new Exception("Giỏ hàng không tồn tại.");
            }

            // 2. Gọi Repo lấy danh sách item chi tiết (đã include Sản phẩm và Ảnh)
            var selectedItems = await _repo.GetSelectedItemsAsync(gioHang.MaGioHang, request.SelectedCartItemIds);

            // 3. Map sang Response (Tái sử dụng logic Map nhưng chỉ với items đã chọn)
            // Tạo một object GioHang tạm thời để dùng lại hàm MapToResponse
            var tempGioHang = new GioHang
            {
                MaGioHang = gioHang.MaGioHang,
                MaKhachHang = gioHang.MaKhachHang,
                NgayCapNhat = DateTime.Now,
                ChiTietGioHangs = selectedItems // Chỉ gán những item đã chọn
            };

            return MapToResponse(tempGioHang);
        }
    }
}