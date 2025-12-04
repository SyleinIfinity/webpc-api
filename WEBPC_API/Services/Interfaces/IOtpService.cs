namespace WEBPC_API.Services.Interfaces
{
    public interface IOtpService
    {
        // Trả về tuple (Thành công/Thất bại, Message lỗi nếu có)
        Task<(bool IsSuccess, string Message)> SendOtpAsync(string email);
        Task<(bool IsSuccess, string Message)> VerifyOtpAsync(string email, string otpCode);
    }
}