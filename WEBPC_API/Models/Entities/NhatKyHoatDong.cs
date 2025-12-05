using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBPC_API.Models.Entities
{
    [Table("NhatKyHoatDong")] // Quan trọng: Tên này phải trùng tên bảng trong SQL
    public class NhatKyHoatDong
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string HanhDong { get; set; }

        [Required]
        public string MoTa { get; set; }

        // Tên cột này phải trùng tên cột trong SQL
        public int? MaNhanVien { get; set; }

        [ForeignKey("MaNhanVien")]
        public virtual NhanVien NhanVien { get; set; }

        public DateTime ThoiGian { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string IPThucHien { get; set; }
    }
}