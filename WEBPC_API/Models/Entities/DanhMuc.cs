using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBPC_API.Models.Entities
{
    [Table("DanhMuc")]
    public class DanhMuc
    {
        [Key]
        public int MaDanhMuc { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [MaxLength(100)]
        public string TenDanhMuc { get; set; }

        [MaxLength(255)]
        public string MoTa { get; set; }

        // Relationship: Một danh mục có nhiều sản phẩm
        // JsonIgnore để tránh vòng lặp khi query dữ liệu
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<SanPham> SanPhams { get; set; }
    }
}