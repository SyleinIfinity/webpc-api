using System.ComponentModel.DataAnnotations;

namespace WEBPC_API.Models.DTOs.Requests
{
    public class TaoDonHangRequest
    {
        public int MaKhachHang { get; set; }
        public string NguoiNhan { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChiGiaoHang { get; set; }

        // [MỚI] Client phải chọn phương thức thanh toán ("COD" hoặc "VietQR")
        [Required]
        public string PhuongThucThanhToan { get; set; }

        // [MỚI] Danh sách các ID (MaChiTietGioHang) được chọn để thanh toán
        // Client bắt buộc phải gửi cái này
        public List<int> SelectedCartItemIds { get; set; } = new List<int>();
    }
}