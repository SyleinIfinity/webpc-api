using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBPC_API.Models.Entities
{
    [Table("OtpLog")]
    public class OtpLog
    {
        [Key]
        [Column("maLog")]
        public int MaLog { get; set; }

        [Required]
        [Column("email")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [Column("maOTP")]
        [StringLength(10)]
        public string MaOTP { get; set; }

        [Column("thoiGianTao")]
        public DateTime ThoiGianTao { get; set; } = DateTime.Now;

        [Column("thoiGianHetHan")]
        public DateTime ThoiGianHetHan { get; set; }

        [Column("trangThai")]
        [StringLength(20)]
        public string TrangThai { get; set; } = "ConHan"; // ConHan, HetHan, DaSuDung
    }
}