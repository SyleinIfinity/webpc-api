using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;

namespace WEBPC_API.Repositories.Implements
{
    public class OtpRepository : IOtpRepository
    {
        private readonly DataContext _context;

        public OtpRepository(DataContext context)
        {
            _context = context;
        }

        public async Task AddOtpAsync(OtpLog otpLog)
        {
            _context.OtpLogs.Add(otpLog);
            // Khi SaveChanges, Trigger SQL sẽ tự động update các dòng cũ thành 'HetHan'
            await _context.SaveChangesAsync();
        }

        public async Task<OtpLog?> GetLatestOtpByEmailAsync(string email)
        {
            return await _context.OtpLogs
                .Where(x => x.Email == email)
                .OrderByDescending(x => x.ThoiGianTao)
                .FirstOrDefaultAsync();
        }

        public async Task<OtpLog?> GetValidOtpAsync(string email, string otpCode)
        {
            return await _context.OtpLogs
                .Where(x => x.Email == email
                         && x.MaOTP == otpCode
                         && x.TrangThai == "ConHan")
                .FirstOrDefaultAsync();
        }

        public async Task UpdateOtpAsync(OtpLog otpLog)
        {
            _context.OtpLogs.Update(otpLog);
            await _context.SaveChangesAsync();
        }
    }
}