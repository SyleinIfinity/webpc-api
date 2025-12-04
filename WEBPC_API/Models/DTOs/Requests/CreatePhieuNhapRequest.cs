using System.ComponentModel.DataAnnotations;

namespace WEBPC_API.Models.DTOs.Requests
{
    public class CreatePhieuNhapRequest
    {
        [Required]
        public int MaNhanVienNhap { get; set; }
        public string? GhiChu { get; set; }

        [Required]
        public List<ChiTietPhieuNhapItem> ChiTiet { get; set; }
    }

    public class ChiTietPhieuNhapItem
    {
        [Required]
        public int MaSanPham { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng nhập phải lớn hơn 0")]
        public int SoLuongNhap { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá nhập không được âm")]
        public decimal GiaNhap { get; set; }
    }
}