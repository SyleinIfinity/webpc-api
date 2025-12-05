using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Models.DTOs.Casso;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // =================================================================================
        // 1. API LẤY MÃ QR THANH TOÁN
        // URL: GET /api/payment/get-qr/{maDonHang}
        // =================================================================================
        [HttpGet("get-qr/{maDonHang}")]
        public async Task<IActionResult> GetPaymentQr(int maDonHang)
        {
            try
            {
                // Gọi Service để tạo QR dựa trên số tiền của đơn hàng
                var result = await _paymentService.CreatePaymentQr(maDonHang);

                if (result == null)
                {
                    return BadRequest(new { message = "Không thể tạo mã QR lúc này. Vui lòng thử lại sau." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Trả về lỗi nếu đơn hàng không tồn tại hoặc có lỗi hệ thống
                return BadRequest(new { message = ex.Message });
            }
        }

        // =================================================================================
        // 2. WEBHOOK ĐỂ CASSO GỌI TỰ ĐỘNG (AUTO BANKING)
        // URL: POST /api/payment/webhook-casso
        // Lưu ý: API này không dành cho Frontend gọi, mà dành cho Server của Casso gọi
        // =================================================================================
        [HttpPost("webhook-casso")]
        public async Task<IActionResult> HandleCassoWebhook([FromBody] CassoWebhookData data)
        {
            try
            {
                // 1. Lấy "Secure-Token" từ Header mà Casso gửi kèm
                // Token này dùng để xác minh xem có đúng là Casso gọi không (hay là hacker)
                Request.Headers.TryGetValue("secure-token", out var secureToken);

                if (string.IsNullOrEmpty(secureToken))
                {
                    return Unauthorized(new { message = "Missing Secure Token" });
                }

                // 2. Đẩy vào Service để xử lý logic (Tìm đơn hàng -> Cập nhật đã thanh toán)
                await _paymentService.ProcessCassoWebhook(data, secureToken.ToString());

                // 3. QUAN TRỌNG: Casso yêu cầu phản hồi JSON cụ thể này để biết đã nhận tin thành công
                // Nếu không trả về đúng format này, Casso sẽ gửi lại liên tục (spam) vì tưởng lỗi
                return Ok(new { error = 0, message = "success" });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ra Console để debug
                Console.WriteLine($"[WEBHOOK ERROR]: {ex.Message}");

                // Vẫn trả về success để Casso không retry (tránh spam server khi gặp lỗi logic nội bộ)
                // Nhưng thực tế là xử lý thất bại
                return Ok(new { error = 0, message = "success" });
            }
        }
    }
}