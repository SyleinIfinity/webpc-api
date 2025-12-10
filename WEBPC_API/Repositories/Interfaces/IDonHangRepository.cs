using WEBPC_API.Models.Entities;

namespace WEBPC_API.Repositories.Interfaces
{
    public interface IDonHangRepository
    {
        Task<DonHang?> GetByIdAsync(int id);
        Task<GiaoDichThanhToan?> GetTransactionByOrderIdAsync(int orderId);
        Task UpdateAsync(DonHang donHang);
        Task SaveChangesAsync();

        // --- BỔ SUNG THÊM 2 HÀM NÀY ---
        Task<IEnumerable<DonHang>> GetAllAsync(); // Để xem danh sách
        Task<DonHang> AddAsync(DonHang donHang);  // Để tạo đơn mới

        Task<IEnumerable<DonHang>> GetByKhachHangIdAsync(int maKhachHang);
    }
}