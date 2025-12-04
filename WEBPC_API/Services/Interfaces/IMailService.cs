namespace WEBPC_API.Services.Interfaces
{
    public interface IMailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    }
}