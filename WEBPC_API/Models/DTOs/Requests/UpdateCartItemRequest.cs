using System.ComponentModel.DataAnnotations;

namespace WEBPC_API.Models.DTOs.Requests
{
    public class UpdateCartItemRequest
    {
        [Required]
        public int MaKhachHang { get; set; }

        [Required]
        public int MaSanPham { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuongMoi { get; set; }
    }
}