namespace WEBPC_API.Models.DTOs.Responses
{
    public class SoDiaChiResponse
    {
        public int MaSoDiaChi { get; set; }
        public int MaKhachHang { get; set; }
        public string TenNguoiNhan { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChiCuThe { get; set; }

        public string TinhThanhId { get; set; }
        public string TenTinhThanh { get; set; }

        public string QuanHuyenId { get; set; }
        public string TenQuanHuyen { get; set; }

        public string PhuongXaId { get; set; }
        public string TenPhuongXa { get; set; }

        public bool MacDinh { get; set; }

        // Helper property: Trả về chuỗi địa chỉ full để tiện hiển thị
        public string DiaChiDayDu => $"{DiaChiCuThe}, {TenPhuongXa}, {TenQuanHuyen}, {TenTinhThanh}";
    }
}