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

        // Thêm dòng này vào Interface
        Task RemoveCartItemsAsync(int maGioHang, List<int> cartItemIds);

        Task<List<ChiTietGioHang>> GetSelectedItemsAsync(int maGioHang, List<int> listChiTietId);
    }
}