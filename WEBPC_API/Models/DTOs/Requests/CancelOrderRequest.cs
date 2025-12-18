using System.ComponentModel.DataAnnotations;

namespace WEBPC_API.Models.DTOs.Requests
{
    public class CancelOrderRequest
    {
        [Required]
        public int MaKhachHang { get; set; }

        public string LyDoHuy { get; set; } = "Khách hàng chủ động hủy";
    }
}