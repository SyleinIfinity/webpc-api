using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WEBPC_API.Models.Entities
{
    [Table("ThongSoKyThuat")]
    public class ThongSoKyThuat
    {
        [Key]
        public int MaThongSo { get; set; }

        [Required]
        public int MaSanPham { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenThongSo { get; set; }

        [Required]
        [MaxLength(255)]
        public string GiaTri { get; set; }

        // Navigation Property (Quan hệ N-1 với SanPham)
        [ForeignKey("MaSanPham")]
        [JsonIgnore] // Tránh loop khi serialize JSON
        public SanPham SanPham { get; set; }
    }
}