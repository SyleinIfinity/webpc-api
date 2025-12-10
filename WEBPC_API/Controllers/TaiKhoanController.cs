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
        // 1. API ĐĂNG NHẬP
        // URL: POST api/TaiKhoan/login
        // ==========================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userService.Login(request);

            if (result == null)
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });
            }

            return Ok(result);
        }

        // ==========================================================
        // 2. API LẤY DANH SÁCH TÀI KHOẢN
        // URL: GET api/TaiKhoan
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _userService.GetAllTaiKhoans();
            return Ok(data);
        }

        // ==========================================================
        // 3. API ĐỔI MẬT KHẨU
        // URL: PATCH api/TaiKhoan/{id}/change-password
        // Body: "MatKhauMoi123" (Gửi dạng chuỗi JSON: "string")
        // ==========================================================
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

        // ==========================================================
        // 4. API CẬP NHẬT ẢNH ĐẠI DIỆN (AVATAR) - [MỚI BỔ SUNG]
        // URL: PATCH api/TaiKhoan/update-avatar/{id}
        // Content-Type: multipart/form-data
        // Key form: "file" (là file ảnh)
        // ==========================================================
        [HttpPatch("update-avatar/{id}")]
        public async Task<IActionResult> UpdateAvatar(int id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Vui lòng chọn file ảnh hợp lệ." });

                // Gọi Service xử lý (Xóa ảnh cũ -> Up ảnh mới -> Lưu DB)
                var newUrl = await _userService.UpdateAvatarAsync(id, file);

                return Ok(new
                {
                    message = "Cập nhật ảnh đại diện thành công.",
                    url = newUrl // Trả về link ảnh mới để Frontend hiển thị ngay
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==========================================================
        // 5. API CẬP NHẬT TRẠNG THÁI (KHÓA/MỞ) - [MỚI BỔ SUNG]
        // URL: PATCH api/TaiKhoan/update-status/{id}
        // Body JSON: { "trangThai": "Locked" } hoặc { "trangThai": "Active" }
        // ==========================================================
        [HttpPatch("update-status/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.TrangThai))
                    return BadRequest(new { message = "Trạng thái không được để trống." });

                await _userService.UpdateStatusAsync(id, request.TrangThai);

                return Ok(new { message = $"Đã cập nhật trạng thái thành: {request.TrangThai}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class UpdateStatusRequest
    {
        public string TrangThai { get; set; } // "Active" hoặc "Locked"
    }
}
