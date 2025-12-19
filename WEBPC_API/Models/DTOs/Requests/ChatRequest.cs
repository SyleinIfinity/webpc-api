namespace WEBPC_API.Models.DTOs.Requests
{
    public class ChatRequest
    {
        public int MaKhachHang { get; set; } // Để biết ai đang hỏi mà lấy dữ liệu
        public string Message { get; set; }  // Câu hỏi, VD: "Tháng này anh tốn bao nhiêu tiền?"
    }
}