namespace WEBPC_API.Models.DTOs.Responses
{
    public class KhuyenMaiKhachHangResponse
    {
        public int MaKMKH { get; set; }
        public int MaKhuyenMai { get; set; }
        public int MaKhachHang { get; set; }
        public string MaCodeKM { get; set; }      // Lấy từ bảng cha để tiện hiển thị
        public string TenChuongTrinh { get; set; } // Lấy từ bảng cha
        public bool DaSuDung { get; set; }
        public DateTime? NgayThuThap { get; set; }
    }
}