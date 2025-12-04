using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using WEBPC_API.Data;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;

namespace WEBPC_API.Repositories.Implements
{
    public class NhanVienRepository : INhanVienRepository
    {
        private readonly DataContext _context;

        public NhanVienRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<NhanVien>> GetAllAsync()
        {
            // Sử dụng Include để lấy luôn thông tin bảng liên kết (Join)
            return await _context.NhanViens
                .Include(nv => nv.VaiTro)
                .Include(nv => nv.TaiKhoan)
                .ToListAsync();
        }

        public async Task<NhanVien?> GetByIdAsync(int id)
        {
            return await _context.NhanViens
                .Include(nv => nv.VaiTro)
                .Include(nv => nv.TaiKhoan)
                .FirstOrDefaultAsync(nv => nv.MaNhanVien == id);
        }

        public async Task<NhanVien?> GetByCodeAsync(string code)
        {
            return await _context.NhanViens
                .Include(nv => nv.VaiTro)
                .Include(nv => nv.TaiKhoan)
                .FirstOrDefaultAsync(nv => nv.MaCodeNhanVien == code);
        }

        public async Task AddAsync(NhanVien nhanVien)
        {
            _context.NhanViens.Add(nhanVien);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(NhanVien nhanVien)
        {
            _context.NhanViens.Update(nhanVien);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien != null)
            {
                _context.NhanViens.Remove(nhanVien);
                await _context.SaveChangesAsync();
            }
        }
    }
}