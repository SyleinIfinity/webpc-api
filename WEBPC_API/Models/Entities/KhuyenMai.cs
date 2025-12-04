using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBPC_API.Models.Entities
{
    [Table("KhuyenMai")]
    public class KhuyenMai
    {
        [Key]
        public int MaKhuyenMai { get; set; }

        [Required]
        [MaxLength(50)]
        public string MaCodeKM { get; set; }

        [Required]
        [MaxLength(255)]
        public string TenChuongTrinh { get; set; }

        public decimal GiaTriGiam { get; set; }

        [Required]
        [MaxLength(20)]
        public string LoaiGiam { get; set; } // 'DIRECT' hoặc 'PERCENT'

        public decimal DonHangToiThieu { get; set; } = 0;

        public decimal? GiamToiDa { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime NgayKetThuc { get; set; }

        public int SoLuongConLai { get; set; } = 0;

        // Relationship
        public ICollection<KhuyenMaiKhachHang> KhuyenMaiKhachHangs { get; set; }
    }
}