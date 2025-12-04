using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WEBPC_API.Models.Entities
{
    [Table("ChiTietGioHang")]
    public class ChiTietGioHang
    {
        [Key]
        public int MaChiTietGioHang { get; set; }

        public int MaGioHang { get; set; }

        public int MaSanPham { get; set; }

        public int SoLuong { get; set; } = 1;

        // Relationship
        [ForeignKey("MaGioHang")]
        [JsonIgnore]
        public virtual GioHang GioHang { get; set; }

        [ForeignKey("MaSanPham")]
        public virtual SanPham SanPham { get; set; }
    }
}