namespace WEBPC_API.Models.DTOs.Responses
{
    public class NhanVienResponse
    {
        public int MaNhanVien { get; set; }
        public string MaCodeNhanVien { get; set; }
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }

        // Thông tin từ bảng VaiTro
        public int MaVaiTro { get; set; }
        public string TenVaiTro { get; set; }

        // Thông tin từ bảng TaiKhoan
        public int MaTaiKhoan { get; set; }
        public string TenDangNhap { get; set; }
        public string Email { get; set; }
        public string TrangThaiTaiKhoan { get; set; }
    }
}