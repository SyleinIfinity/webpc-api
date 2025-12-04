using System.ComponentModel.DataAnnotations;

namespace WEBPC_API.Models.DTOs.Requests
{
    public class ThuThapKhuyenMaiRequest
    {
        [Required]
        public int MaKhuyenMai { get; set; }

        [Required]
        public int MaKhachHang { get; set; }
    }
}