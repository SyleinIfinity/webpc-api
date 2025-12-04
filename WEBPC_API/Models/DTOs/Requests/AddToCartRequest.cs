using System.ComponentModel.DataAnnotations;

namespace WEBPC_API.Models.DTOs.Requests
{
    public class AddToCartRequest
    {
        [Required]
        public int MaKhachHang { get; set; } // Trong thực tế sẽ lấy từ Token, ở đây để test

        [Required]
        public int MaSanPham { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; } = 1;
    }
}