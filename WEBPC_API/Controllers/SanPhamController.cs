using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanPhamController : ControllerBase
    {
        private readonly ISanPhamService _service;

        public SanPhamController(ISanPhamService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllProductsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetProductByIdAsync(id);
            if (result == null) return NotFound("Không tìm thấy sản phẩm");
            return Ok(result);
        }

        // GET: api/SanPham/danhmuc/{maDanhMuc}
        [HttpGet("danhmuc/{maDanhMuc}")]
        public async Task<IActionResult> GetByCategoryId(int maDanhMuc)
        {
            var result = await _service.GetProductsByCategoryIdAsync(maDanhMuc);

            // Nếu muốn trả về 404 khi danh mục không có sản phẩm nào thì mở comment dưới
             if (result == null || !result.Any()) 
                return NotFound("Không có sản phẩm nào thuộc danh mục này.");

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateProductRequest request)
        {
            // [FromForm] là bắt buộc để nhận file upload
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.CreateProductAsync(request);
            if (result) return StatusCode(201, "Tạo sản phẩm thành công");
            return StatusCode(500, "Có lỗi xảy ra");
        }

        [HttpPatch("{id}")] // <--- Đổi từ Put sang Patch
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProductRequest request)
        {
            var result = await _service.UpdateProductAsync(id, request);
            if (!result) return NotFound("Không tìm thấy sản phẩm để cập nhật");
            return Ok("Cập nhật thành công");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteProductAsync(id);
            if (!result) return NotFound("Không tìm thấy sản phẩm để xóa");
            return Ok("Xóa thành công");
        }
    }
}