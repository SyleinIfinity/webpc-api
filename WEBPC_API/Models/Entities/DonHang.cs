using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBPC_API.Models.Entities
{
    [Table("DonHang")]
    public class DonHang
    {
        [Key]
        public int maDonHang { get; set; }

        [Required]
        [StringLength(50)]
        public string maCodeDonHang { get; set; } // Mã đơn hàng (VD: DH8372)

        public int maKhachHang { get; set; }
        [ForeignKey("maKhachHang")]
        public virtual KhachHang KhachHang { get; set; }

        public int? maNhanVienDuyet { get; set; }
        [ForeignKey("maNhanVienDuyet")]
        public virtual NhanVien NhanVien { get; set; }

        public DateTime ngayDat { get; set; } = DateTime.Now;

        public decimal tongTien { get; set; }

        [StringLength(50)]
        public string trangThai { get; set; } = "ChoXacNhan";
        // Các trạng thái: ChoXacNhan, ChoThanhToan, DaThanhToan, DangGiao, HoanThanh, Huy

        [Required]
        [StringLength(255)]
        public string diaChiGiaoHang { get; set; }

        [Required]
        [StringLength(15)]
        public string soDienThoaiGiao { get; set; }

        [Required]
        [StringLength(100)]
        public string nguoiNhan { get; set; }

        public decimal phiVanChuyen { get; set; } = 0;

        // Navigation Property: Một đơn hàng có nhiều chi tiết
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
    }
}