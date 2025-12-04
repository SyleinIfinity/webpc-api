using System.ComponentModel.DataAnnotations;

namespace WEBPC_API.Models.DTOs.Requests
{
    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        public string TenDanhMuc { get; set; }

        public string MoTa { get; set; }
    }
}