using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace WEBPC_API.Helpers
{
    // 1. Class để hứng dữ liệu cấu hình từ appsettings.json
    public class VietQrSettings
    {
        public string BaseUrl { get; set; }      // https://api.vietqr.io/v2/generate
        public string ClientId { get; set; }     // Để trống nếu dùng bản miễn phí
        public string ApiKey { get; set; }       // Để trống nếu dùng bản miễn phí
        public string BankId { get; set; }       // Ví dụ: 970415 (VietinBank)
        public string AccountNo { get; set; }    // Số tài khoản
        public string AccountName { get; set; }  // Tên chủ tài khoản
        public string Template { get; set; }     // compact2
    }

    // 2. DTO: Dữ liệu gửi đi (Request) để yêu cầu tạo QR
    public class VietQrRequest
    {
        public string accountNo { get; set; }
        public string accountName { get; set; }
        public int acqId { get; set; }           // Mã BIN ngân hàng (BankId)
        public int amount { get; set; }          // Số tiền cần chuyển
        public string addInfo { get; set; }      // Nội dung chuyển khoản
        public string format { get; set; } = "text";
        public string template { get; set; } = "compact2";
    }

    // 3. DTO: Dữ liệu nhận về (Response) từ VietQR
    public class VietQrResponse
    {
        public string code { get; set; }         // Mã lỗi (00 là thành công)
        public string desc { get; set; }         // Mô tả lỗi
        public VietQrData data { get; set; }     // Dữ liệu chính
    }

    public class VietQrData
    {
        public string qrCode { get; set; }       // Chuỗi mã QR (ít dùng)
        public string qrDataURL { get; set; }    // Link ảnh Base64 (Dùng cái này để hiện ảnh)
    }

    // 4. Helper xử lý chính
    public class VietQrHelper
    {
        private readonly HttpClient _httpClient;
        private readonly VietQrSettings _settings;

        // Constructor: Nhận HttpClient và Cấu hình (Dependency Injection)
        public VietQrHelper(HttpClient httpClient, IOptions<VietQrSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        /// <summary>
        /// Hàm gọi API VietQR để tạo mã thanh toán
        /// </summary>
        /// <param name="amount">Số tiền cần thanh toán</param>
        /// <param name="content">Nội dung chuyển khoản (VD: DH12345)</param>
        /// <returns>Đối tượng chứa link ảnh QR</returns>
        public async Task<VietQrResponse> GenerateQrAsync(double amount, string content)
        {
            // Chuẩn bị dữ liệu gửi đi
            var requestData = new VietQrRequest
            {
                accountNo = _settings.AccountNo,
                accountName = _settings.AccountName,
                acqId = int.Parse(_settings.BankId), // Chuyển chuỗi "970415" thành số
                amount = (int)amount,
                addInfo = content,
                template = _settings.Template
            };

            // Chuyển dữ liệu sang JSON
            var jsonContent = JsonConvert.SerializeObject(requestData);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Xóa Header cũ (nếu có) để tránh lỗi
            _httpClient.DefaultRequestHeaders.Clear();

            // Nếu trong appsettings có điền Key thì mới thêm vào Header (Dành cho bản Pro)
            // Nếu bạn để trống (bản Free) thì đoạn này sẽ được bỏ qua -> Code vẫn chạy tốt
            if (!string.IsNullOrEmpty(_settings.ClientId) && !string.IsNullOrEmpty(_settings.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("x-client-id", _settings.ClientId);
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
            }

            try
            {
                // Gọi POST sang VietQR
                var response = await _httpClient.PostAsync(_settings.BaseUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<VietQrResponse>(responseString);
                }

                // Nếu lỗi mạng hoặc lỗi server
                return new VietQrResponse
                {
                    code = "ERROR",
                    desc = $"HTTP Error: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                // Bắt lỗi ngoại lệ (mất mạng, sai url...)
                return new VietQrResponse
                {
                    code = "EXCEPTION",
                    desc = ex.Message
                };
            }
        }
    }
}