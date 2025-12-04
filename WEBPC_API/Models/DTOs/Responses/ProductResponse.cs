using System.Collections.Generic;

namespace WEBPC_API.Models.DTOs.Responses
{
    public class ProductResponse
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public decimal GiaBan { get; set; }
        public decimal? GiaKhuyenMai { get; set; } // Thêm
        public bool TrangThai { get; set; } // Thêm
        public int SoLuongTon { get; set; }
        public string MoTa { get; set; }

        // Trả về tên thay vì mã ID để Client dễ hiển thị
        public string TenDanhMuc { get; set; }

        // Danh sách hình ảnh
        public List<ImageResponse> DanhSachAnh { get; set; }
    }
}