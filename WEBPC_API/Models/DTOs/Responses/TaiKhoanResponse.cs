using System;

namespace WEBPC_API.Models.DTOs.Responses
{
    public class TaiKhoanResponse
    {
        public int MaTaiKhoan { get; set; }
        public string TenDangNhap { get; set; }
        public string Email { get; set; }
        public string TrangThai { get; set; }
        public DateTime NgayTao { get; set; }
        // Có thể thêm thông tin role nếu cần, nhưng ở đây giữ cơ bản theo bảng
    }
}