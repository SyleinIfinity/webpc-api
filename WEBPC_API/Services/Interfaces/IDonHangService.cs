using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Models.Entities;

namespace WEBPC_API.Services.Interfaces
{
    public interface IDonHangService
    {
        Task<DonHang> CreateOrderAsync(TaoDonHangRequest request);

        // --- CÁC HÀM GET MỚI ---
        Task<List<DonHangResponse>> GetAllOrdersAsync();
        Task<DonHangResponse?> GetOrderByIdAsync(int id);
        Task<List<DonHangResponse>> GetOrdersByCustomerAsync(int maKhachHang);
        Task CancelOrderAsync(int orderId, CancelOrderRequest request);
    }
}