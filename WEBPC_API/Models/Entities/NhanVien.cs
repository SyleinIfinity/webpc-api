using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBPC_API.Models.Entities
{
    [Table("NhanVien")]
    public class NhanVien
    {
        [Key]
        public int MaNhanVien { get; set; }
        [Required]
        [MaxLength(20)]
        public string MaCodeNhanVien { get; set; }
        [Required]
        [MaxLength(100)]
        public string HoTen { get; set; }
        [MaxLength(15)]
        public string? SoDienThoai { get; set; }

        public int MaTaiKhoan { get; set; }
        public int MaVaiTro { get; set; }

        // Relationship
        [ForeignKey("MaTaiKhoan")]
        public TaiKhoan TaiKhoan { get; set; }
        [ForeignKey("MaVaiTro")]
        public VaiTro VaiTro { get; set; }
    }
}