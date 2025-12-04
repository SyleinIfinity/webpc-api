using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThongSoKyThuatController : ControllerBase
    {
        private readonly IThongSoKyThuatService _service;

        public ThongSoKyThuatController(IThongSoKyThuatService service)
        {
            _service = service;
        }

        // GET: api/ThongSoKyThuat/sanpham/{maSanPham}
        [HttpGet("sanpham/{maSanPham}")]
        public async Task<IActionResult> GetByProductId(int maSanPham)
        {
            var result = await _service.GetByProductIdAsync(maSanPham);
            return Ok(result);
        }

        // POST: api/ThongSoKyThuat
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateThongSoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.CreateAsync(request);
                return CreatedAtAction(nameof(GetByProductId), new { maSanPham = result.MaSanPham }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // PATCH: api/ThongSoKyThuat/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateThongSoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _service.UpdateAsync(id, request);
            if (!success)
                return NotFound(new { message = "Không tìm thấy thông số kỹ thuật này." });

            return Ok(new { message = "Cập nhật thành công." });
        }

        // DELETE: api/ThongSoKyThuat/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = "Không tìm thấy thông số kỹ thuật này." });

            return Ok(new { message = "Xóa thành công." });
        }
    }
}