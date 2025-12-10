using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WEBPC_API.Models.Entities
{
    [Table("SoDiaChi")]
    public class SoDiaChi
    {
        [Key]
        public int MaSoDiaChi { get; set; } // Đây là thuộc tính bị báo lỗi thiếu

        public int MaKhachHang { get; set; }

        // --- BỔ SUNG 2 TRƯỜNG MỚI ---
        [Required]
        [MaxLength(100)]
        public string TenNguoiNhan { get; set; }

        [Required]
        [MaxLength(15)]
        public string SoDienThoai { get; set; }


        [Required]
        [MaxLength(255)]
        public string DiaChiCuThe { get; set; } // Đây là thuộc tính bị báo lỗi thiếu

        // Các ID địa chính (để gọi API)
        [MaxLength(20)]
        public string TinhThanhId { get; set; }

        [MaxLength(20)]
        public string QuanHuyenId { get; set; }

        [MaxLength(20)]
        public string PhuongXaId { get; set; }

        // Các Tên địa chính (để hiển thị)
        [MaxLength(100)]
        public string TenTinhThanh { get; set; }

        [MaxLength(100)]
        public string TenQuanHuyen { get; set; }

        [MaxLength(100)]
        public string TenPhuongXa { get; set; }

        public bool MacDinh { get; set; } = false;

        // Relationship
        [ForeignKey("MaKhachHang")]
        [JsonIgnore]
        public KhachHang KhachHang { get; set; }
    }
}