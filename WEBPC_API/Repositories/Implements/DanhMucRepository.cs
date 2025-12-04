using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Models.Entities;
using WEBPC_API.Models.Interfaces;

namespace WEBPC_API.Models.Repositories
{
    public class DanhMucRepository : IDanhMucRepository
    {
        private readonly DataContext _context;

        public DanhMucRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DanhMuc>> GetAllAsync()
        {
            // Include SanPhams để đếm số lượng (nếu cần)
            return await _context.DanhMucs.Include(d => d.SanPhams).ToListAsync();
        }

        public async Task<DanhMuc> GetByIdAsync(int id)
        {
            return await _context.DanhMucs.Include(d => d.SanPhams)
                                 .FirstOrDefaultAsync(d => d.MaDanhMuc == id);
        }

        public async Task<int> AddAsync(DanhMuc danhMuc)
        {
            _context.DanhMucs.Add(danhMuc);
            await _context.SaveChangesAsync();
            return danhMuc.MaDanhMuc;
        }

        public async Task UpdateAsync(DanhMuc danhMuc)
        {
            _context.DanhMucs.Update(danhMuc);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var dm = await _context.DanhMucs.FindAsync(id);
            if (dm != null)
            {
                _context.DanhMucs.Remove(dm);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsNameExistsAsync(string name)
        {
            return await _context.DanhMucs.AnyAsync(d => d.TenDanhMuc == name);
        }

        public async Task<bool> HasProductsAsync(int id)
        {
            return await _context.SanPhams.AnyAsync(s => s.MaDanhMuc == id);
        }
    }
}