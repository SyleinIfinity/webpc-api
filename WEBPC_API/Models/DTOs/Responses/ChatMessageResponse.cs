namespace WEBPC_API.Models.DTOs.Responses
{
    public class ChatMessageResponse
    {
        public string Role { get; set; }    // "user" (người dùng) hoặc "model" (bot)
        public string Content { get; set; } // Nội dung tin nhắn
        public DateTime Time { get; set; }  // Thời gian
    }
}