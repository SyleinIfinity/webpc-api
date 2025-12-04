using WEBPC_API.Models.Entities;

namespace WEBPC_API.Repositories.Interfaces
{
    public interface IOtpRepository
    {
        Task AddOtpAsync(OtpLog otpLog);
        Task<OtpLog?> GetLatestOtpByEmailAsync(string email); // Lấy mã mới nhất bất kể trạng thái để check 30s
        Task<OtpLog?> GetValidOtpAsync(string email, string otpCode); // Lấy mã khớp và còn hạn
        Task UpdateOtpAsync(OtpLog otpLog);
    }
}