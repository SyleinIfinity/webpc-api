using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBPC_API.Models.Entities
{
    [Table("VaiTro")]
    public class VaiTro
    {
        [Key]
        public int MaVaiTro { get; set; }
        [Required]
        [MaxLength(50)]
        public string TenVaiTro { get; set; }
        [MaxLength(255)]
        public string? MoTa { get; set; }

        // Relationship
        public ICollection<NhanVien> NhanViens { get; set; }
    }
}