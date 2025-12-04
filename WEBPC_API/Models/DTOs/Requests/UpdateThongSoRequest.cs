using System.ComponentModel.DataAnnotations;

namespace WEBPC_API.Models.DTOs.Requests
{
    public class UpdateThongSoRequest
    {
        [Required]
        public string TenThongSo { get; set; }

        [Required]
        public string GiaTri { get; set; }
    }
}