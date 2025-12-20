using Microsoft.AspNetCore.Mvc;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeminiTestController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public GeminiTestController(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        // API: GET /api/GeminiTest/check-models
        [HttpGet("check-models")]
        public async Task<IActionResult> CheckAvailableModels()
        {
            try
            {
                // 1. Lấy Key
                var apiKey = _configuration["Gemini:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    return BadRequest(new { message = "Chưa cấu hình Gemini:ApiKey trong Environment hoặc appsettings.json" });
                }

                // 2. Gọi API liệt kê danh sách Model của Google
                // URL này không tốn tiền, chỉ để check quyền
                var url = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                // 3. Trả về nguyên gốc kết quả từ Google
                if (response.IsSuccessStatusCode)
                {
                    return Ok(new
                    {
                        status = "Thành công (200)",
                        description = "Key hoạt động tốt. Dưới đây là danh sách Model bạn được dùng:",
                        google_response = System.Text.Json.JsonDocument.Parse(content)
                    });
                }
                else
                {
                    return StatusCode((int)response.StatusCode, new
                    {
                        status = $"Lỗi ({response.StatusCode})",
                        description = "Google từ chối Key này. Xem chi tiết bên dưới:",
                        google_error = content
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi Server nội bộ: " + ex.Message });
            }
        }
    }
}