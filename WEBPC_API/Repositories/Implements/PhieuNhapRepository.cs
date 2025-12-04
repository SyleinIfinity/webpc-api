using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;

namespace WEBPC_API.Repositories.Implements
{
    public class PhieuNhapRepository : IPhieuNhapRepository
    {
        private readonly DataContext _context;

        public PhieuNhapRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<PhieuNhap>> GetAllAsync()
        {
            return await _context.PhieuNhaps
                .Include(p => p.NhanVien)
                .Include(p => p.ChiTietPhieuNhaps).ThenInclude(ct => ct.SanPham)
                .OrderByDescending(p => p.NgayNhap)
                .ToListAsync();
        }

        public async Task<PhieuNhap?> GetByIdAsync(int id)
        {
            return await _context.PhieuNhaps
                .Include(p => p.NhanVien)
                .Include(p => p.ChiTietPhieuNhaps).ThenInclude(ct => ct.SanPham)
                .FirstOrDefaultAsync(p => p.MaPhieuNhap == id);
        }

        public async Task<PhieuNhap?> GetByCodeAsync(string code)
        {
            return await _context.PhieuNhaps
                .Include(p => p.NhanVien)
                .Include(p => p.ChiTietPhieuNhaps)
                .FirstOrDefaultAsync(p => p.MaCodePhieu == code);
        }

        public async Task<PhieuNhap> CreateAsync(PhieuNhap phieuNhap)
        {
            _context.PhieuNhaps.Add(phieuNhap);
            await _context.SaveChangesAsync();
            return phieuNhap;
        }

        public async Task<PhieuNhap> UpdateAsync(PhieuNhap phieuNhap)
        {
            _context.PhieuNhaps.Update(phieuNhap);
            await _context.SaveChangesAsync();
            return phieuNhap;
        }

        public async Task DeleteAsync(int id)
        {
            var phieuNhap = await _context.PhieuNhaps.FindAsync(id);
            if (phieuNhap != null)
            {
                _context.PhieuNhaps.Remove(phieuNhap);
                await _context.SaveChangesAsync();
            }
        }
    }
}