using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WEBPC_API.Models.Entities
{
    [Table("ChiTietPhieuNhap")]
    public class ChiTietPhieuNhap
    {
        [Key]
        public int MaChiTietPhieuNhap { get; set; }

        public int MaPhieuNhap { get; set; }

        public int MaSanPham { get; set; }

        public int SoLuongNhap { get; set; }

        public decimal GiaNhap { get; set; }

        // Relationship
        [ForeignKey("MaPhieuNhap")]
        [JsonIgnore]
        public virtual PhieuNhap PhieuNhap { get; set; }

        [ForeignKey("MaSanPham")]
        public virtual SanPham SanPham { get; set; }
    }
}