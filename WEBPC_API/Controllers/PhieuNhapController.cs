using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhieuNhapController : ControllerBase
    {
        private readonly IPhieuNhapService _service;

        public PhieuNhapController(IPhieuNhapService service)
        {
            _service = service;
        }

        // GET: api/PhieuNhap
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllPhieuNhapAsync();
            return Ok(result);
        }

        // GET: api/PhieuNhap/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetPhieuNhapByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Không tìm thấy phiếu nhập" });
            }
        }

        // POST: api/PhieuNhap
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePhieuNhapRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _service.CreatePhieuNhapAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.MaPhieuNhap }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // PATCH: api/PhieuNhap/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePhieuNhapRequest request)
        {
            try
            {
                var result = await _service.UpdatePhieuNhapAsync(id, request);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Không tìm thấy phiếu nhập để cập nhật" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // DELETE: api/PhieuNhap/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeletePhieuNhapAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy phiếu nhập để xóa" });

            return Ok(new { message = "Xóa phiếu nhập thành công (Đã cập nhật lại kho)" });
        }
    }
}