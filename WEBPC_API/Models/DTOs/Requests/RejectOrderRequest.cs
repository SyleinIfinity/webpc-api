namespace WEBPC_API.Models.DTOs.Requests
{
    public class RejectOrderRequest
    {
        public int OrderId { get; set; } // Thêm dòng này để khớp với Service
        public string LyDoTuChoi { get; set; } // Giữ nguyên tên này
    }
}