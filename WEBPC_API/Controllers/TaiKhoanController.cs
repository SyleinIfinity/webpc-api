using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaiKhoanController : ControllerBase
    {
        private readonly IUserService _userService;

        public TaiKhoanController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _userService.GetAllTaiKhoans();
            return Ok(data);
        }

        // API đổi mật khẩu: PATCH api/taikhoan/{id}/change-password
        // Body gửi lên là một chuỗi string (JSON format), ví dụ: "MatKhauMoi123"
        [HttpPatch("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                return BadRequest(new { message = "Mật khẩu mới không được để trống." });
            }

            var result = await _userService.ChangePassword(id, newPassword);
            if (result)
            {
                return Ok(new { message = "Đổi mật khẩu thành công." });
            }
            return NotFound(new { message = "Không tìm thấy tài khoản." });
        }
    }
}