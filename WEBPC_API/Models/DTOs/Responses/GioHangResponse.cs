namespace WEBPC_API.Models.DTOs.Responses
{
    public class GioHangResponse
    {
        public int MaGioHang { get; set; }
        public int MaKhachHang { get; set; }
        public DateTime NgayCapNhat { get; set; }
        public decimal TongTienHang { get; set; } // Tổng tiền tạm tính
        public int TongSoLuongSanPham { get; set; }
        public List<ChiTietGioHangResponse> ChiTiet { get; set; }
    }

    public class ChiTietGioHangResponse
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; } // Ảnh đại diện
        public decimal DonGia { get; set; }
        public decimal GiaKhuyenMai { get; set; } // Nếu có logic khuyến mãi
        public int SoLuong { get; set; }
        public decimal ThanhTien => (GiaKhuyenMai > 0 ? GiaKhuyenMai : DonGia) * SoLuong;
    }
}