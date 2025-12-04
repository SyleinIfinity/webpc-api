using System.ComponentModel.DataAnnotations;

namespace WEBPC_API.Models.DTOs.Requests
{
    public class CreateKhuyenMaiRequest
    {
        [Required]
        [MaxLength(50)]
        public string MaCodeKM { get; set; }

        [Required]
        [MaxLength(255)]
        public string TenChuongTrinh { get; set; }

        [Required]
        public decimal GiaTriGiam { get; set; }

        [Required]
        [RegularExpression("DIRECT|PERCENT", ErrorMessage = "Loại giảm chỉ được là 'DIRECT' hoặc 'PERCENT'")]
        public string LoaiGiam { get; set; }

        public decimal DonHangToiThieu { get; set; } = 0;
        public decimal? GiamToiDa { get; set; }

        [Required]
        public DateTime NgayBatDau { get; set; }

        [Required]
        public DateTime NgayKetThuc { get; set; }

        public int SoLuongConLai { get; set; }
    }
}