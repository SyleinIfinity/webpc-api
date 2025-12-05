using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBPC_API.Models.Entities
{
    [Table("ChiTietDonHang")]
    public class ChiTietDonHang
    {
        [Key]
        public int maChiTietDonHang { get; set; }

        public int maDonHang { get; set; }
        [ForeignKey("maDonHang")]
        public virtual DonHang DonHang { get; set; }

        public int maSanPham { get; set; }
        [ForeignKey("maSanPham")]
        public virtual SanPham SanPham { get; set; }

        public int soLuong { get; set; }

        public decimal donGiaLucMua { get; set; } // Giá sản phẩm tại thời điểm mua (tránh việc sau này giá đổi)

        public decimal thanhTien { get; set; } // = soLuong * donGiaLucMua
    }
}