using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HinhAnhSanPhamController : ControllerBase
    {
        private readonly IHinhAnhSanPhamService _service;

        public HinhAnhSanPhamController(IHinhAnhSanPhamService service)
        {
            _service = service;
        }

        // GET: api/HinhAnhSanPham/product/5
        // Lấy tất cả ảnh của sản phẩm ID = 5
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var result = await _service.GetImagesByProductId(productId);
            return Ok(result);
        }

        // POST: api/HinhAnhSanPham/product/5
        // Upload thêm 1 ảnh vào sản phẩm ID = 5
        [HttpPost("product/{productId}")]
        public async Task<IActionResult> UploadImage(int productId, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Vui lòng chọn file ảnh");

            try
            {
                var result = await _service.AddImageToProductAsync(productId, file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/HinhAnhSanPham/10
        // Xóa bức ảnh có ID = 10 (Xóa cả trên Cloud lẫn DB)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var result = await _service.DeleteImageAsync(id);
            if (!result) return NotFound("Không tìm thấy hình ảnh");
            return Ok("Đã xóa hình ảnh thành công");
        }

        // ==========================================================
        // API XÓA TẤT CẢ ẢNH CỦA SẢN PHẨM
        // URL: DELETE api/HinhAnhSanPham/delete-all/{productId}
        // ==========================================================
        [HttpDelete("delete-all/{productId}")]
        public async Task<IActionResult> DeleteAllByProductId(int productId)
        {
            try
            {
                var result = await _service.DeleteAllImagesByProductIdAsync(productId);

                if (result)
                {
                    return Ok(new { message = "Đã xóa toàn bộ hình ảnh của sản phẩm." });
                }

                return NotFound(new { message = "Sản phẩm không có hình ảnh nào hoặc không tồn tại." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}