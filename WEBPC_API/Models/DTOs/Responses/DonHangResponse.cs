namespace WEBPC_API.Models.DTOs.Responses
{
    public class DonHangResponse
    {
        public int MaDonHang { get; set; }
        public string MaCodeDonHang { get; set; }
        public int MaKhachHang { get; set; }
        public DateTime NgayDat { get; set; }
        public string TrangThai { get; set; }

        // [MỚI] Trả về phương thức để Client hiển thị
        public string PhuongThucThanhToan { get; set; }

        // Thông tin giao hàng
        public string NguoiNhan { get; set; }
        public string SoDienThoaiGiao { get; set; }
        public string DiaChiGiaoHang { get; set; }

        // Tài chính
        public decimal PhiVanChuyen { get; set; }
        public decimal TongTien { get; set; }

        // Danh sách sản phẩm
        public List<ChiTietDonHangResponse> ChiTiet { get; set; }
    }

    public class ChiTietDonHangResponse
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; } // Ảnh đại diện
        public int SoLuong { get; set; }
        public decimal DonGiaLucMua { get; set; }
        public decimal ThanhTien { get; set; }
    }
}