using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhMucController : ControllerBase
    {
        private readonly IDanhMucService _service;

        public DanhMucController(IDanhMucService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllCategoriesAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetCategoryByIdAsync(id);
            if (result == null) return NotFound("Không tìm thấy danh mục");
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _service.CreateCategoryAsync(request);
                return StatusCode(201, "Tạo danh mục thành công");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")] // <--- Đổi từ Put sang Patch
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest request)
        {
            try
            {
                var result = await _service.UpdateCategoryAsync(id, request);
                if (!result) return NotFound("Không tìm thấy danh mục");
                return Ok("Cập nhật thành công");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _service.DeleteCategoryAsync(id);
            if (message == "OK")
                return Ok("Xóa thành công");

            // Nếu lỗi do logic (vd: còn sản phẩm) thì trả về BadRequest
            return BadRequest(message);
        }
    }
}