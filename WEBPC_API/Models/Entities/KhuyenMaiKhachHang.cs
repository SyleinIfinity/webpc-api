using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WEBPC_API.Models.Entities
{
    [Table("KhuyenMaiKhachHang")]
    public class KhuyenMaiKhachHang
    {
        [Key]
        public int MaKMKH { get; set; }

        public int MaKhuyenMai { get; set; }

        public int MaKhachHang { get; set; }

        public bool DaSuDung { get; set; } = false;

        public DateTime? NgayThuThap { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("MaKhuyenMai")]
        [JsonIgnore]
        public KhuyenMai KhuyenMai { get; set; }

        [ForeignKey("MaKhachHang")]
        [JsonIgnore]
        public KhachHang KhachHang { get; set; }
    }
}