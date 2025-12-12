using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Models.Entities;
using WEBPC_API.Models.Interfaces;
using WEBPC_API.Repositories.Interfaces;

namespace WEBPC_API.Repositories.Implements
{
    public class DanhMucRepository : IDanhMucRepository
    {
        private readonly DataContext _context;

        public DanhMucRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<DanhMuc>> GetAllAsync()
        {
            // Include DanhMucCha để lấy tên của cha
            // Include DanhMucCons để đếm số lượng con
            return await _context.DanhMucs
                .Include(d => d.DanhMucCha)
                .Include(d => d.DanhMucCons)
                .ToListAsync();
        }

        public async Task<DanhMuc?> GetByIdAsync(int id)
        {
            return await _context.DanhMucs
                .Include(d => d.DanhMucCha)
                .Include(d => d.DanhMucCons)
                .FirstOrDefaultAsync(d => d.MaDanhMuc == id);
        }

        public async Task<DanhMuc> CreateAsync(DanhMuc danhMuc)
        {
            _context.DanhMucs.Add(danhMuc);
            await _context.SaveChangesAsync();
            return danhMuc;
        }

        public async Task<DanhMuc> UpdateAsync(DanhMuc danhMuc)
        {
            _context.DanhMucs.Update(danhMuc);
            await _context.SaveChangesAsync();
            return danhMuc;
        }

        public async Task DeleteAsync(int id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc != null)
            {
                _context.DanhMucs.Remove(danhMuc);
                await _context.SaveChangesAsync();
            }
        }
    }
}