using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Models.Entities; // Thêm dòng này nếu cần dùng DonHang
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WEBPC_API.Services.Interfaces
{
    public interface IOrderAdminService
    {
        // Lấy danh sách đơn hàng
        Task<List<DonHang>> GetAllOrdersAsync();

        // Duyệt đơn
        Task<OrderProcessResponse> ApproveOrderAsync(int orderId, int staffId);

        // Từ chối / Hủy đơn (Chỉ nhận 2 tham số)
        Task<OrderProcessResponse> RejectOrderAsync(RejectOrderRequest request, int staffId);

        // --- BỔ SUNG HÀM NÀY ĐỂ SỬA LỖI COMPILER ---
        Task<OrderProcessResponse> ConfirmRefundAsync(int orderId, int staffId);
    }
}