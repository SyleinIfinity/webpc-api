namespace WEBPC_API.Models.DTOs.Requests
{
    public class KhachHangRequest
    {
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }

        // Optional: Nếu muốn tạo luôn tài khoản cho khách
        public string? TenDangNhap { get; set; }
        public string? MatKhau { get; set; }
    }
}