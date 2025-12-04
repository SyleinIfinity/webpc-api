namespace WEBPC_API.Models.DTOs.Responses
{
    public class ImageResponse
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool LaAnhDaiDien { get; set; }
        public string PublicId { get; set; } // Trả về cái này để Frontend biết ID mà gọi lệnh xóa
    }
}