using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;

namespace WEBPC_API.Services.Interfaces
{
    public interface IOrderAdminService
    {
        Task<OrderProcessResponse> RejectOrderAsync(int orderId, RejectOrderRequest request, int nhanVienId);

        Task<OrderProcessResponse> ConfirmRefundAsync(int orderId, int adminId);
    }
}