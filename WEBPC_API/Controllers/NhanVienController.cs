using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NhanVienController : ControllerBase
    {
        private readonly IUserService _userService;

        public NhanVienController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _userService.GetAllNhanViens();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _userService.GetNhanVienById(id);
            if (data == null) return NotFound(new { message = "Không tìm thấy nhân viên." });
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NhanVienRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Gọi Service trả về Tuple (IsSuccess, Message)
            var result = await _userService.CreateNhanVien(request);

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
        public async Task<IActionResult> Update(int id, [FromBody] NhanVienRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userService.UpdateNhanVien(id, request);
            if (result)
            {
                return Ok(new { message = "Cập nhật thông tin nhân viên thành công." });
            }
            return NotFound(new { message = "Không tìm thấy nhân viên để cập nhật." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userService.DeleteNhanVien(id);
            if (result)
            {
                return Ok(new { message = "Đã xóa nhân viên và tài khoản liên quan." });
            }
            return NotFound(new { message = "Không tìm thấy nhân viên." });
        }
    }
}