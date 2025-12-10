using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WEBPC_API.Models.DTOs.Requests
{
    public class UpdateProductRequest
    {
        public string? TenSanPham { get; set; }

        public decimal? GiaBan { get; set; }

        // --- BỔ SUNG ---
        public decimal? GiaKhuyenMai { get; set; }

        public int? SoLuongTon { get; set; }

        public string? MoTa { get; set; }

        // --- BỔ SUNG (Để null để không bắt buộc gửi) ---
        public bool? TrangThai { get; set; }

        public int? MaDanhMuc { get; set; }

        public int? AnhDaiDienId { get; set; }

        // Danh sách ảnh thêm mới (nếu có)
        public List<IFormFile>? HinhAnhs { get; set; }

        // Danh sách ảnh cũ muốn xóa (nếu có)
        public List<string>? PublicIdsToDelete { get; set; }
    }
}