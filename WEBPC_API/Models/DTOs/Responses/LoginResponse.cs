namespace WEBPC_API.Models.DTOs.Responses
{
    public class LoginResponse
    {
        public int MaNhanVien { get; set; }
        public string HoTen { get; set; }
        public string TenDangNhap { get; set; }
        public int MaVaiTro { get; set; }
        public string TenVaiTro { get; set; }
        public string Token { get; set; } // Chuỗi JWT
    }
}