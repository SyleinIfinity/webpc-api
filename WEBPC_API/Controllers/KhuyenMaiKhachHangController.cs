using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhuyenMaiKhachHangController : ControllerBase
    {
        private readonly IKhuyenMaiKhachHangService _service;

        public KhuyenMaiKhachHangController(IKhuyenMaiKhachHangService service)
        {
            _service = service;
        }

        // Lấy danh sách mã của 1 khách hàng
        [HttpGet("khachhang/{maKhachHang}")]
        public async Task<IActionResult> GetByCustomer(int maKhachHang)
        {
            var data = await _service.GetByKhachHangIdAsync(maKhachHang);
            return Ok(data);
        }

        // Khách hàng thu thập mã (Lưu coupon vào ví)
        [HttpPost("collect")]
        public async Task<IActionResult> Collect([FromBody] ThuThapKhuyenMaiRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var resultMessage = await _service.ThuThapKhuyenMaiAsync(request);

            if (resultMessage == "Thu thập thành công!")
                return Ok(new { message = resultMessage });

            return BadRequest(new { message = resultMessage });
        }

        // Xóa mã khỏi ví (Admin hoặc user xóa)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.XoaKhuyenMaiCuaKhachAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "Đã xóa mã khuyến mãi khỏi ví." });
        }
    }
}