using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WEBPC_API.Models.Entities
{
    [Table("DanhMuc")]
    public class DanhMuc
    {
        [Key]
        public int MaDanhMuc { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenDanhMuc { get; set; }

        [MaxLength(255)]
        public string? MoTa { get; set; }

        // --- BỔ SUNG CẤU HÌNH DANH MỤC CHA ---
        public int? MaDanhMucCha { get; set; }

        [ForeignKey("MaDanhMucCha")]
        [JsonIgnore] // Tránh vòng lặp khi serialize
        public virtual DanhMuc? DanhMucCha { get; set; }

        public virtual ICollection<DanhMuc> DanhMucCons { get; set; }
        // --------------------------------------

        [JsonIgnore]
        public virtual ICollection<SanPham> SanPhams { get; set; }
    }
}