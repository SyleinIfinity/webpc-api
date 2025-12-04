using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using WEBPC_API.Data;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;

namespace WEBPC_API.Repositories.Implements
{
    public class KhachHangRepository : IKhachHangRepository
    {
        private readonly DataContext _context;

        public KhachHangRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<KhachHang>> GetAllAsync()
        {
            // Include TaiKhoan để biết khách hàng này có tài khoản chưa
            return await _context.KhachHangs
                .Include(kh => kh.TaiKhoan)
                .ToListAsync();
        }

        public async Task<KhachHang?> GetByIdAsync(int id)
        {
            return await _context.KhachHangs
                .Include(kh => kh.TaiKhoan)
                .FirstOrDefaultAsync(kh => kh.MaKhachHang == id);
        }

        public async Task<KhachHang?> GetByPhoneAsync(string phone)
        {
            return await _context.KhachHangs
                .Include(kh => kh.TaiKhoan)
                .FirstOrDefaultAsync(kh => kh.SoDienThoai == phone);
        }

        public async Task AddAsync(KhachHang khachHang)
        {
            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(KhachHang khachHang)
        {
            _context.KhachHangs.Update(khachHang);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang != null)
            {
                _context.KhachHangs.Remove(khachHang);
                await _context.SaveChangesAsync();
            }
        }
    }
}