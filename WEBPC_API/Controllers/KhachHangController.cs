using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhachHangController : ControllerBase
    {
        private readonly IUserService _userService;

        public KhachHangController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _userService.GetAllKhachHangs();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] KhachHangRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userService.CreateKhachHang(request);

            if (result.IsSuccess)
            {
                return Ok(new { message = result.Message });
            }
            else
            {
                return BadRequest(new { message = result.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateKhachHangRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userService.UpdateKhachHang(id, request);

            if (result)
            {
                return Ok(new { message = "Cập nhật thông tin khách hàng thành công." });
            }

            return NotFound(new { message = "Không tìm thấy khách hàng." });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                // Gọi Service vừa viết (Lưu ý: Controller này phải Inject IUserService)
                var result = await _userService.GetCustomerByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }
    }
}