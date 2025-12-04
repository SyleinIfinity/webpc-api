using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Services.Business
{
    public class OtpService : IOtpService
    {
        private readonly IOtpRepository _otpRepository;
        private readonly IMailService _mailService;

        public OtpService(IOtpRepository otpRepository, IMailService mailService)
        {
            _otpRepository = otpRepository;
            _mailService = mailService;
        }

        public async Task<(bool IsSuccess, string Message)> SendOtpAsync(string email)
        {
            // 1. Kiểm tra spam (trong vòng 30s không được gửi lại)
            var lastOtp = await _otpRepository.GetLatestOtpByEmailAsync(email);
            if (lastOtp != null)
            {
                var timeDiff = DateTime.Now - lastOtp.ThoiGianTao;
                if (timeDiff.TotalSeconds < 30)
                {
                    return (false, $"Vui lòng đợi {30 - (int)timeDiff.TotalSeconds} giây để gửi lại OTP.");
                }
            }

            // 2. Tạo mã OTP Random 6 ký tự
            string otpCode = new Random().Next(0, 999999).ToString("D6");

            // 3. Tạo Entity
            var newOtp = new OtpLog
            {
                Email = email,
                MaOTP = otpCode,
                ThoiGianTao = DateTime.Now,
                ThoiGianHetHan = DateTime.Now.AddMinutes(5), // Hết hạn sau 5 phút
                TrangThai = "ConHan"
            };

            // 4. Lưu vào DB (Trigger sẽ xử lý các mã cũ)
            await _otpRepository.AddOtpAsync(newOtp);

            // 5. Gửi Email
            string subject = "[WEBPC] Mã xác thực OTP của bạn";
            string body = $"<h3>Mã OTP của bạn là: <b style='color:red; font-size: 20px;'>{otpCode}</b></h3>" +
                          $"<p>Mã này có hiệu lực trong 5 phút.</p>";

            bool mailSent = await _mailService.SendEmailAsync(email, subject, body);

            if (!mailSent)
            {
                return (false, "Không thể gửi email. Vui lòng kiểm tra lại địa chỉ email.");
            }

            return (true, "Mã OTP đã được gửi đến email của bạn.");
        }

        public async Task<(bool IsSuccess, string Message)> VerifyOtpAsync(string email, string otpCode)
        {
            // 1. Lấy mã OTP hợp lệ từ DB
            var otpLog = await _otpRepository.GetValidOtpAsync(email, otpCode);

            // 2. Kiểm tra tồn tại
            if (otpLog == null)
            {
                return (false, "Mã OTP không chính xác hoặc đã bị vô hiệu hóa.");
            }

            // 3. Kiểm tra thời gian hết hạn (Logic code để chắc chắn, dù DB có thể lưu)
            if (otpLog.ThoiGianHetHan < DateTime.Now)
            {
                // Cập nhật trạng thái thành hết hạn nếu chưa update
                otpLog.TrangThai = "HetHan";
                await _otpRepository.UpdateOtpAsync(otpLog);
                return (false, "Mã OTP đã hết hạn.");
            }

            // 4. Thành công -> Đổi trạng thái thành Đã Sử Dụng
            otpLog.TrangThai = "DaSuDung";
            await _otpRepository.UpdateOtpAsync(otpLog);

            return (true, "Xác thực OTP thành công.");
        }
    }
}