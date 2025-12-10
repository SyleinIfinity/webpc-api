using System.ComponentModel.DataAnnotations;

namespace WEBPC_API.Models.DTOs.Requests
{
    public class CreateSoDiaChiRequest
    {
        [Required]
        public int MaKhachHang { get; set; }

        [Required(ErrorMessage = "Tên người nhận là bắt buộc")]
        public string TenNguoiNhan { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        public string SoDienThoai { get; set; }

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