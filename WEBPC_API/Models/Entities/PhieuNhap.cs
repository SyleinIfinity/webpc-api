using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBPC_API.Models.Entities
{
    [Table("PhieuNhap")]
    public class PhieuNhap
    {
        [Key]
        public int MaPhieuNhap { get; set; }

        [Required]
        [MaxLength(50)]
        public string MaCodePhieu { get; set; }

        public DateTime NgayNhap { get; set; } = DateTime.Now;

        public decimal TongTienNhap { get; set; }

        public int MaNhanVienNhap { get; set; }

        public string? GhiChu { get; set; }

        // Relationship
        [ForeignKey("MaNhanVienNhap")]
        public virtual NhanVien NhanVien { get; set; }

        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
    }
}