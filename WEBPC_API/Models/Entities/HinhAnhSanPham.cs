using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBPC_API.Models.Entities
{
    [Table("HinhAnhSanPham")]
    public class HinhAnhSanPham
    {
        [Key]
        [Column("maHinhAnh")]
        public int Id { get; set; }

        public int MaSanPham { get; set; }

        [ForeignKey("MaSanPham")]
        public SanPham SanPham { get; set; }

        [Required]
        public string UrlHinhAnh { get; set; }

        public string PublicId { get; set; }

        // --- BỔ SUNG THUỘC TÍNH NÀY ---
        public bool LaAnhDaiDien { get; set; } = false;
    }
}