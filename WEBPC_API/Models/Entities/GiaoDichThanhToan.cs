using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBPC_API.Models.Entities
{
    [Table("GiaoDichThanhToan")]
    public class GiaoDichThanhToan
    {
        [Key]
        public int maGiaoDich { get; set; }

        public int maDonHang { get; set; }
        [ForeignKey("maDonHang")]
        public virtual DonHang DonHang { get; set; }

        [StringLength(100)]
        public string? maGiaoDichMomo { get; set; } // Lưu mã tham chiếu ngân hàng (TID) khi thanh toán thành công

        [Required]
        [StringLength(20)]
        public string phuongThuc { get; set; } // VD: "VietQR", "COD", "Momo"

        public decimal soTien { get; set; }

        [StringLength(50)]
        public string trangThai { get; set; } = "Pending"; // Pending, Success, Failed

        [StringLength(255)]
        public string? noiDungLoi { get; set; }

        public DateTime ngayTao { get; set; } = DateTime.Now;
    }
}