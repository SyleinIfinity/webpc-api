using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBPC_API.Models.Entities
{
    [Table("GioHang")]
    public class GioHang
    {
        [Key]
        public int MaGioHang { get; set; }

        [Required]
        public int MaKhachHang { get; set; }

        public DateTime NgayCapNhat { get; set; } = DateTime.Now;

        // Relationship
        [ForeignKey("MaKhachHang")]
        public virtual KhachHang KhachHang { get; set; }

        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }
    }
}