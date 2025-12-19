using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBPC_API.Models.Entities
{
    [Table("PhienChat")]
    public class PhienChat
    {
        [Key]
        public int MaPhien { get; set; }

        public int MaKhachHang { get; set; }

        // Thời gian cập nhật cuối cùng (Để tính logic 10 phút)
        public DateTime ThoiGianCapNhat { get; set; } = DateTime.Now;

        // Quan hệ 1-N với chi tiết
        public virtual ICollection<ChiTietPhienChat> ChiTietPhienChats { get; set; }
    }
}