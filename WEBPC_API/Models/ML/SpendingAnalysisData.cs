// File: WEBPC_API/Models/ML/SpendingAnalysisData.cs
namespace WEBPC_API.Models.ML
{
    public class SpendingAnalysisData
    {
        // 1. Tổng quan
        public float TongTienDaChi { get; set; }       // Đã giao (Thành công)
        public float TongTienDangCho { get; set; }     // Chờ duyệt, Đang giao (Tiền đang treo)
        public float TongTienDaHuy { get; set; }       // Đã hủy (Tiền hụt)
        public int SoDonThanhCong { get; set; }
        public int SoDonHuy { get; set; }

        // 2. Danh sách đơn hàng gần đây (để Bot đọc chi tiết)
        public List<DonHangTomTat> DonHangGanDay { get; set; } = new List<DonHangTomTat>();

        public List<SanPhamGoiY> SanPhamShopDangBan { get; set; } = new List<SanPhamGoiY>();
    }

    public class DonHangTomTat
    {
        public DateTime Ngay { get; set; }
        public string TrangThai { get; set; }
        public float SoTien { get; set; }
        public string TenSanPhamChinh { get; set; }
    }

    // [MỚI] Class nhỏ để lưu thông tin sản phẩm
    public class SanPhamGoiY
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public float Gia { get; set; }
        public string DanhMuc { get; set; } // Quan trọng để Bot biết loại hàng
    }
}