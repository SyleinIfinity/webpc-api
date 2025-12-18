using WEBPC_API.Models.Entities;

namespace WEBPC_API.Repositories.Interfaces
{
    public interface IDonHangRepository
    {
        // Hàm thêm đơn hàng mới
        Task<DonHang> AddAsync(DonHang donHang);

        // Các hàm lấy dữ liệu
        Task<DonHang?> GetByIdAsync(int id);
        Task<DonHang?> GetByCodeAsync(string maCode);
        Task<List<DonHang>> GetByKhachHangIdAsync(int maKhachHang);

        Task UpdateAsync(DonHang donHang);
        Task<List<DonHang>> GetAllAsync(); // Lấy tất cả
        Task<GiaoDichThanhToan?> GetTransactionByOrderIdAsync(int orderId);

        // Lấy danh sách tất cả đơn hàng (Kèm chi tiết)
        Task<List<DonHang>> GetAllOrdersFullAsync();

        // Lấy chi tiết 1 đơn hàng theo ID
        Task<DonHang?> GetOrderByIdFullAsync(int id);

        // Lấy danh sách đơn hàng của 1 khách hàng cụ thể (Thường dùng cho App Khách hàng)
        Task<List<DonHang>> GetOrdersByCustomerIdAsync(int maKhachHang);
    }
}