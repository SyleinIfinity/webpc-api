using System.ComponentModel.DataAnnotations;

namespace WEBPC_API.Models.DTOs.Requests
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string MatKhau { get; set; }
    }
}