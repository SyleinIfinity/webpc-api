using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SoDiaChiController : ControllerBase
    {
        private readonly ISoDiaChiService _service;

        public SoDiaChiController(ISoDiaChiService service)
        {
            _service = service;
        }

        // --- NHÓM API LOCATION (Dùng cho dropdown frontend) ---

        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            return Ok(await _service.GetProvincesAsync());
        }

        [HttpGet("districts/{provinceId}")]
        public async Task<IActionResult> GetDistricts(string provinceId)
        {
            return Ok(await _service.GetDistrictsAsync(provinceId));
        }

        [HttpGet("wards/{districtId}")]
        public async Task<IActionResult> GetWards(string districtId)
        {
            return Ok(await _service.GetWardsAsync(districtId));
        }

        // --- NHÓM API CRUD SO DIA CHI ---

        [HttpGet("khachhang/{maKhachHang}")]
        public async Task<IActionResult> GetByCustomer(int maKhachHang)
        {
            return Ok(await _service.GetByKhachHangIdAsync(maKhachHang));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSoDiaChiRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var result = await _service.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.MaSoDiaChi }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSoDiaChiRequest request)
        {
            try
            {
                var success = await _service.UpdateAsync(id, request);
                if (!success) return NotFound();
                return Ok(new { message = "Cập nhật thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "Xóa thành công." });
        }
    }
}