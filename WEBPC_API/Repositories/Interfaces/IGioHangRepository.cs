using WEBPC_API.Models.Entities;

namespace WEBPC_API.Repositories.Interfaces
{
    public interface IGioHangRepository
    {
        // Tìm giỏ hàng theo ID Khách Hàng (bao gồm cả chi tiết và thông tin sản phẩm)
        Task<GioHang?> GetByKhachHangIdAsync(int maKhachHang);

        // Tạo mới giỏ hàng (nếu chưa có)
        Task<GioHang> CreateAsync(GioHang gioHang);

        // Thêm hoặc cập nhật chi tiết giỏ hàng
        Task AddOrUpdateItemAsync(ChiTietGioHang item);

        // Xóa 1 sản phẩm khỏi giỏ
        Task RemoveItemAsync(int maGioHang, int maSanPham);

        // Xóa toàn bộ giỏ hàng (Clear cart)
        Task ClearCartAsync(int maGioHang);

        Task SaveChangesAsync();
    }
}