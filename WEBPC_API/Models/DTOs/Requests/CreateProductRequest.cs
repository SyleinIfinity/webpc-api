using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WEBPC_API.Models.DTOs.Requests
{
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string TenSanPham { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
        public decimal GiaBan { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn không hợp lệ")]
        public int SoLuongTon { get; set; }

        public string MoTa { get; set; }

        [Required]
        public int MaDanhMuc { get; set; }

        // Danh sách file ảnh upload lên (Dùng IFormFile để nhận file từ Postman/Frontend)
        public List<IFormFile> HinhAnhs { get; set; }

        public decimal? GiaKhuyenMai { get; set; } // Thêm dòng này
        public bool TrangThai { get; set; } = true; // Thêm dòng này
    }
}