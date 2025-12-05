using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Controllers
{
    [Route("api/admin/orders")]
    [ApiController]
    public class OrderAdminController : ControllerBase
    {
        private readonly IOrderAdminService _orderAdminService;

        public OrderAdminController(IOrderAdminService orderAdminService)
        {
            _orderAdminService = orderAdminService;
        }

        // 1. API Từ chối đơn (Dành cho Sales + Admin)
        [HttpPost("reject/{id}")]
        [Authorize(Roles = "NhanVien,Admin")] // Cả 2 đều được vào
        public async Task<IActionResult> RejectOrder(int id, [FromBody] RejectOrderRequest request)
        {
            var userId = GetUserId();
            var result = await _orderAdminService.RejectOrderAsync(id, request, userId);

            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // 2. API Hoàn tiền (Dành RIÊNG cho Admin)
        [HttpPost("refund-confirm/{id}")]
        [Authorize(Roles = "Admin")] // CHỈ ADMIN MỚI ĐƯỢC VÀO
        public async Task<IActionResult> ConfirmRefund(int id)
        {
            var userId = GetUserId(); // Lúc này userId là của Admin
            var result = await _orderAdminService.ConfirmRefundAsync(id, userId);

            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // Hàm phụ trợ lấy ID từ token
        private int GetUserId()
        {
            var claim = User.FindFirst("Id") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
}