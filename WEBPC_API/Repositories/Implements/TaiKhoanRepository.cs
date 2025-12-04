using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using WEBPC_API.Data;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;

namespace WEBPC_API.Repositories.Implements
{
    public class TaiKhoanRepository : ITaiKhoanRepository
    {
        private readonly DataContext _context;

        public TaiKhoanRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<TaiKhoan>> GetAllAsync()
        {
            return await _context.TaiKhoans.ToListAsync();
        }

        public async Task<TaiKhoan?> GetByIdAsync(int id)
        {
            return await _context.TaiKhoans.FindAsync(id);
        }

        public async Task<TaiKhoan?> GetByUsernameAsync(string username)
        {
            return await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.TenDangNhap == username);
        }

        public async Task<TaiKhoan?> GetByEmailAsync(string email)
        {
            return await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.Email == email);
        }

        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            return await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == username);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.TaiKhoans.AnyAsync(t => t.Email == email);
        }

        public async Task AddAsync(TaiKhoan taiKhoan)
        {
            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TaiKhoan taiKhoan)
        {
            _context.TaiKhoans.Update(taiKhoan);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan != null)
            {
                _context.TaiKhoans.Remove(taiKhoan);
                await _context.SaveChangesAsync();
            }
        }
    }
}