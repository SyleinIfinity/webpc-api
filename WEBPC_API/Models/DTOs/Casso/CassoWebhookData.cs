namespace WEBPC_API.Models.DTOs.Casso
{
    // Class chính hứng toàn bộ JSON Casso gửi về
    public class CassoWebhookData
    {
        public int error { get; set; }
        public string message { get; set; }
        public List<CassoTransaction> data { get; set; }
    }

    // Class chi tiết từng giao dịch bên trong
    public class CassoTransaction
    {
        public int id { get; set; }
        public string tid { get; set; }         // Mã tham chiếu ngân hàng
        public string description { get; set; } // Nội dung chuyển khoản (QUAN TRỌNG NHẤT)
        public decimal amount { get; set; }     // Số tiền chuyển
        public string cusum_balance { get; set; }
        public string when { get; set; }        // Thời gian giao dịch
        public string bank_sub_acc_id { get; set; }
    }
}