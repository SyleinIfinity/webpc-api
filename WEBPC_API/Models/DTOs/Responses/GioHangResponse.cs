using System.Collections.Generic;
using Newtonsoft.Json;

namespace WEBPC_API.Models.DTOs.Responses
{
    public class GioHangResponse
    {
        public int MaGioHang { get; set; }
        public int MaKhachHang { get; set; }
        public DateTime NgayCapNhat { get; set; }
        public decimal TongTienHang { get; set; }
        public int TongSoLuongSanPham { get; set; }

        [JsonProperty("chiTiet")] // Đặt tên json là 'chiTiet' như bạn muốn
        public List<ChiTietGioHangResponse> ChiTietGioHangs { get; set; }
    }

    public class ChiTietGioHangResponse
    {
        // [MỚI] Thêm trường này để Client dùng làm ID chọn
        public int MaChiTietGioHang { get; set; }

        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public decimal DonGia { get; set; }
        public decimal GiaKhuyenMai { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; }
    }
}