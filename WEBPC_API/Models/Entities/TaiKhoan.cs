using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBPC_API.Models.Entities
{
    [Table("TaiKhoan")]
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }
        [Required]
        [MaxLength(50)]
        public string TenDangNhap { get; set; }
        [Required]
        [MaxLength(255)]
        public string MatKhauHash { get; set; }
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }
        public string? AnhDaiDien { get; set; }
        [MaxLength(20)]
        public string TrangThai { get; set; } = "Active";
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Relationship
        public NhanVien? NhanVien { get; set; }
        public KhachHang? KhachHang { get; set; }
    }
}