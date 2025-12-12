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

        // GET: api/DanhMuc
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllCategoriesAsync();
            return Ok(result);
        }

        // GET: api/DanhMuc/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetCategoryByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Không tìm thấy danh mục" });
            }
        }

        // POST: api/DanhMuc
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _service.CreateCategoryAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.MaDanhMuc }, result);
            }
            catch (Exception ex)
            {
                // Bắt lỗi nghiệp vụ (VD: Mã cha không tồn tại)
                return BadRequest(new { message = ex.Message });
            }
        }

        // PATCH: api/DanhMuc/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest request)
        {
            try
            {
                var result = await _service.UpdateCategoryAsync(id, request);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Không tìm thấy danh mục để cập nhật" });
            }
            catch (Exception ex)
            {
                // Bắt lỗi nghiệp vụ (VD: Chọn cha là chính nó)
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/DanhMuc/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _service.DeleteCategoryAsync(id);
                if (!success) return NotFound(new { message = "Không tìm thấy danh mục để xóa" });

                return Ok(new { message = "Xóa danh mục thành công" });
            }
            catch (Exception ex)
            {
                // Bắt lỗi nghiệp vụ (VD: Không được xóa vì đang chứa danh mục con)
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}