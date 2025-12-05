using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WEBPC_API.Models.DTOs.Requests; // Quan trọng: Để nhận được LoginRequest
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

        // ==========================================================
        // 1. API ĐĂNG NHẬP (MỚI BỔ SUNG)
        // Đường dẫn: POST api/TaiKhoan/login
        // ==========================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Kiểm tra dữ liệu gửi lên (Validation)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Gọi Service xử lý đăng nhập
            var result = await _userService.Login(request);

            // Nếu kết quả trả về null nghĩa là sai thông tin
            if (result == null)
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });
            }

            // Đăng nhập thành công -> Trả về Token và thông tin User
            return Ok(result);
        }

        // ==========================================================
        // 2. CÁC API CŨ (GIỮ NGUYÊN)
        // ==========================================================

        // Lấy danh sách tài khoản
        // GET: api/TaiKhoan
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _userService.GetAllTaiKhoans();
            return Ok(data);
        }

        // Đổi mật khẩu
        // PATCH: api/TaiKhoan/{id}/change-password
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