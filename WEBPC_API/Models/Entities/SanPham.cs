using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WEBPC_API.Models.Entities
{
    [Table("SanPham")]
    public class SanPham
    {
        [Key]
        public int MaSanPham { get; set; }

        [Required]
        [MaxLength(255)]
        public string TenSanPham { get; set; }

        public decimal GiaBan { get; set; }

        // --- BỔ SUNG THUỘC TÍNH NÀY ---
        public decimal? GiaKhuyenMai { get; set; }

        public int SoLuongTon { get; set; }

        public string MoTa { get; set; }

        // --- BỔ SUNG THUỘC TÍNH NÀY ---
        public bool TrangThai { get; set; } = true;

        public int MaDanhMuc { get; set; }

        [ForeignKey("MaDanhMuc")]
        [JsonIgnore]
        public virtual DanhMuc DanhMuc { get; set; }

        public ICollection<HinhAnhSanPham> HinhAnhs { get; set; }
    }
}