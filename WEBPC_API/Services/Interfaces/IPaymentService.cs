using WEBPC_API.Helpers;
using WEBPC_API.Models.DTOs.Casso;

namespace WEBPC_API.Services.Interfaces
{
    public interface IPaymentService
    {
        // 1. Hàm tạo QR thanh toán cho một đơn hàng cụ thể
        Task<VietQrResponse> CreatePaymentQr(int maDonHang);

        // 2. Hàm xử lý khi Casso báo có tiền về (Webhook)
        Task ProcessCassoWebhook(CassoWebhookData webhookData, string secureToken);
    }
}