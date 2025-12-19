using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Services.Business;
using WEBPC_API.Models.DTOs.Requests; // Chứa class ChatRequest

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBotController : ControllerBase
    {
        private readonly GeminiService _geminiService;

        public ChatBotController(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        // API: Chat với trợ lý tài chính
        // POST: api/ChatBot/ask
        [HttpPost("ask")]
        public async Task<IActionResult> AskBot([FromBody] ChatRequest req)
        {
            // Validate đầu vào
            if (req == null || string.IsNullOrWhiteSpace(req.Message))
            {
                return BadRequest(new { message = "Vui lòng nhập câu hỏi." });
            }

            if (req.MaKhachHang <= 0)
            {
                return BadRequest(new { message = "Mã khách hàng không hợp lệ." });
            }

            try
            {
                // Gọi Service xử lý (Service đã tự lo Retry, tự lo Prompt)
                string answer = await _geminiService.ChatWithGemini(req.MaKhachHang, req.Message);

                return Ok(new
                {
                    bot_reply = answer
                });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                return StatusCode(500, new { error = "Lỗi xử lý Chatbot: " + ex.Message });
            }
        }

        // API: GET /api/ChatBot/history?maKhachHang=1
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int maKhachHang)
        {
            if (maKhachHang <= 0)
                return BadRequest(new { message = "Mã khách hàng không hợp lệ." });

            try
            {
                var history = await _geminiService.GetChatHistoryAsync(maKhachHang);
                return Ok(history);
                // Trả về mảng JSON: [{ role: "user", content: "..." }, { role: "model", content: "..." }]
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi lấy lịch sử: " + ex.Message });
            }
        }
    }
}