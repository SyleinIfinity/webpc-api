namespace WEBPC_API.Models.DTOs.Responses
{
    public class PhieuNhapResponse
    {
        public int MaPhieuNhap { get; set; }
        public string MaCodePhieu { get; set; }
        public DateTime NgayNhap { get; set; }
        public decimal TongTienNhap { get; set; }
        public string TenNhanVien { get; set; }
        public string GhiChu { get; set; }
        public List<ChiTietPhieuNhapResponse> ChiTiet { get; set; }
    }

    public class ChiTietPhieuNhapResponse
    {
        public int MaChiTietPhieuNhap { get; set; }
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public int SoLuongNhap { get; set; }
        public decimal GiaNhap { get; set; }
        public decimal ThanhTien => SoLuongNhap * GiaNhap;
    }
}