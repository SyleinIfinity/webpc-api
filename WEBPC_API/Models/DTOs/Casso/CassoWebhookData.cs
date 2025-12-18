namespace WEBPC_API.Models.DTOs.Casso
{
    // Class chính hứng toàn bộ JSON Casso gửi về
    public class CassoWebhookData
    {
        public int error { get; set; }
        public string? message { get; set; }
        public List<CassoTransaction>? data { get; set; }
    }

    public class CassoTransaction
    {
        public int id { get; set; }
        public string? tid { get; set; }
        public string? description { get; set; }
        public decimal amount { get; set; }
        public decimal? cusum_balance { get; set; }
        public string? when { get; set; }
        public string? bank_sub_acc_id { get; set; }
    }

}