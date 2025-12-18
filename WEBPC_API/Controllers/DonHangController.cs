using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangController : ControllerBase
    {
        // Inject Service, không Inject Repository trực tiếp
        private readonly IDonHangService _donHangService;

        public DonHangController(IDonHangService donHangService)
        {
            _donHangService = donHangService;
        }

        // POST: api/DonHang/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] TaoDonHangRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newOrder = await _donHangService.CreateOrderAsync(request);

                // Trả về thông tin đơn hàng vừa tạo
                return Ok(new
                {
                    success = true,
                    message = "Tạo đơn hàng thành công",
                    maDonHang = newOrder.maDonHang,
                    maCode = newOrder.maCodeDonHang,
                    tongTien = newOrder.tongTien
                });
            }
            catch (Exception ex)
            {
                // Trả về lỗi (VD: Hết hàng, Giỏ hàng trống...)
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // [MỚI] GET: api/DonHang (Lấy tất cả - Dành cho Admin)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _donHangService.GetAllOrdersAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // [MỚI] GET: api/DonHang/{id} (Lấy chi tiết 1 đơn)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _donHangService.GetOrderByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = "Không tìm thấy đơn hàng" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // [MỚI] GET: api/DonHang/customer/{customerId} (Lấy lịch sử đơn của khách)
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            try
            {
                var result = await _donHangService.GetOrdersByCustomerAsync(customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _donHangService.CancelOrderAsync(id, request);
                return Ok(new
                {
                    success = true,
                    message = "Hủy đơn hàng thành công."
                });
            }
            catch (Exception ex)
            {
                // Trả về lỗi 400 Bad Request nếu vi phạm logic (ví dụ: hủy đơn đã thanh toán)
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}