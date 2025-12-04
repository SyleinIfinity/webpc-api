namespace WEBPC_API.Models.DTOs.Responses
{
    public class KhachHangResponse
    {
        public int MaKhachHang { get; set; }
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }

        // Thông tin tài khoản liên kết (nếu có)
        public int? MaTaiKhoan { get; set; }
        public string? TenDangNhap { get; set; }
        public bool CoTaiKhoan { get; set; } // Flag để frontend dễ hiển thị
    }
}