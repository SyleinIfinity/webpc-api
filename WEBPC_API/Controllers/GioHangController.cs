using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GioHangController : ControllerBase
    {
        private readonly IGioHangService _service;

        public GioHangController(IGioHangService service)
        {
            _service = service;
        }

        // GET: api/GioHang/{maKhachHang}
        [HttpGet("{maKhachHang}")]
        public async Task<IActionResult> GetCart(int maKhachHang)
        {
            try
            {
                var result = await _service.GetGioHangByKhachHangAsync(maKhachHang);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST: api/GioHang/add
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _service.AddToCartAsync(request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // PATCH: api/GioHang/update-item
        [HttpPatch("update-item")]
        public async Task<IActionResult> UpdateItem([FromBody] UpdateCartItemRequest request)
        {
            try
            {
                var result = await _service.UpdateCartItemAsync(request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // DELETE: api/GioHang/remove-item?maKhachHang=1&maSanPham=10
        [HttpDelete("remove-item")]
        public async Task<IActionResult> RemoveItem([FromQuery] int maKhachHang, [FromQuery] int maSanPham)
        {
            try
            {
                var result = await _service.RemoveFromCartAsync(maKhachHang, maSanPham);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // DELETE: api/GioHang/clear/{maKhachHang}
        [HttpDelete("clear/{maKhachHang}")]
        public async Task<IActionResult> ClearCart(int maKhachHang)
        {
            var result = await _service.ClearCartAsync(maKhachHang);
            if (result)
                return Ok(new { message = "Đã xóa toàn bộ giỏ hàng" });
            return BadRequest(new { message = "Lỗi khi xóa giỏ hàng" });
        }
    }
}