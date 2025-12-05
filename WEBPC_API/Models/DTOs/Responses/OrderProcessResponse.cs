namespace WEBPC_API.Models.DTOs.Responses
{
    public class OrderProcessResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int MaDonHang { get; set; }
        public string TrangThaiMoi { get; set; }
    }
}