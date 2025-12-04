namespace WEBPC_API.Models.DTOs.Requests
{
    public class TaiKhoanRequest
    {
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; } // Raw password
        public string Email { get; set; }
        public string TrangThai { get; set; } = "Active";
    }
}