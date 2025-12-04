using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBPC_API.Models.Entities
{
    [Table("KhachHang")]
    public class KhachHang
    {
        [Key]
        public int MaKhachHang { get; set; }
        [Required]
        [MaxLength(100)]
        public string HoTen { get; set; }
        [Required]
        [MaxLength(15)]
        public string SoDienThoai { get; set; }
        [MaxLength(100)]
        public string? Email { get; set; }

        public int? MaTaiKhoan { get; set; }

        // Relationship
        [ForeignKey("MaTaiKhoan")]
        public TaiKhoan? TaiKhoan { get; set; }
    }
}