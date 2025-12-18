using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Services.Interfaces;
using WEBPC_API.Models.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WEBPC_API.Controllers
{
    [Route("api/order-admin")]
    [ApiController]
    // [Authorize(Roles = "Admin,Staff")] // Mở ra khi muốn chặn quyền
    public class OrderAdminController : ControllerBase
    {
        private readonly IOrderAdminService _orderAdminService;

        public OrderAdminController(IOrderAdminService orderAdminService)
        {
            _orderAdminService = orderAdminService;
        }

        // 1. GET: Lấy danh sách đơn hàng
        [HttpGet("list")]
        public async Task<IActionResult> GetAllOrders()
        {
            var list = await _orderAdminService.GetAllOrdersAsync();
            return Ok(list);
        }

        // 2. POST: Duyệt đơn hàng
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> ApproveOrder(int id)
        {
            // Lấy ID nhân viên từ Token (nếu chưa có Token thì tạm fix cứng = 1)
            int nhanVienId = GetCurrentUserId();

            var result = await _orderAdminService.ApproveOrderAsync(id, nhanVienId);

            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // 3. POST: Từ chối / Hủy đơn hàng
        [HttpPost("reject/{id}")]
        public async Task<IActionResult> RejectOrder(int id, [FromBody] RejectOrderRequest request)
        {
            int nhanVienId = GetCurrentUserId();

            // [THẦY SỬA LỖI TẠI ĐÂY]: 
            // Gán ID từ URL vào trong Object Request luôn
            request.OrderId = id;

            // Gọi Service với ĐÚNG 2 tham số (Request và StaffId)
            var result = await _orderAdminService.RejectOrderAsync(request, nhanVienId);

            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // 4. POST: Xác nhận hoàn tiền (Dành cho Admin)
        [HttpPost("confirm-refund/{id}")]
        public async Task<IActionResult> ConfirmRefund(int id)
        {
            int nhanVienId = GetCurrentUserId();

            // Bây giờ Interface đã có hàm này, lỗi đỏ sẽ hết
            var result = await _orderAdminService.ConfirmRefundAsync(id, nhanVienId);

            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // Hàm phụ lấy ID nhân viên từ Token
        private int GetCurrentUserId()
        {
            // Nếu chưa cấu hình Authen thì trả về 1 (Admin mặc định)
            if (User == null || !User.Identity.IsAuthenticated) return 1;

            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null && int.TryParse(idClaim.Value, out int id))
            {
                return id;
            }
            return 1;
        }
    }
}