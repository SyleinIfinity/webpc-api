using System.ComponentModel.DataAnnotations;

namespace WEBPC_API.Models.DTOs.Requests
{
    public class CreateSoDiaChiRequest
    {
        [Required]
        public int MaKhachHang { get; set; }

        [Required]
        public string DiaChiCuThe { get; set; }

        [Required]
        public string TinhThanhId { get; set; }

        [Required]
        public string QuanHuyenId { get; set; }

        [Required]
        public string PhuongXaId { get; set; }

        public bool MacDinh { get; set; } = false;
    }
}