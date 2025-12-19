using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBPC_API.Models.Entities
{
    [Table("ChiTietPhienChat")]
    public class ChiTietPhienChat
    {
        [Key]
        public int Id { get; set; }

        public int MaPhien { get; set; }
        [ForeignKey("MaPhien")]
        public virtual PhienChat PhienChat { get; set; }

        [Required]
        public string CauHoi { get; set; } // Người gửi

        [Required]
        public string CauTraLoi { get; set; } // Bot trả lời

        public DateTime ThoiGian { get; set; } = DateTime.Now;
    }
}