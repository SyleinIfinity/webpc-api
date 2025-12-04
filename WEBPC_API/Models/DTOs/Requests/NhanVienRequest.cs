namespace WEBPC_API.Models.DTOs.Requests
{
    public class NhanVienRequest
    {
        // Thông tin cá nhân
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public int MaVaiTro { get; set; }

        // Thông tin tài khoản (Để tạo đồng thời)
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
        public string Email { get; set; }
    }
}