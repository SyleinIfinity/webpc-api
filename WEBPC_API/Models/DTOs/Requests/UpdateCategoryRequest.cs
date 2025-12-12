namespace WEBPC_API.Models.DTOs.Requests
{
    public class UpdateCategoryRequest
    {
        public string? TenDanhMuc { get; set; }
        public string? MoTa { get; set; }
        // --- BỔ SUNG ---
        public int? MaDanhMucCha { get; set; }
    }
}