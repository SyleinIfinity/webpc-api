using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;

namespace WEBPC_API.Repositories.Implements
{
    public class KhuyenMaiRepository : IKhuyenMaiRepository
    {
        private readonly DataContext _context;
        public KhuyenMaiRepository(DataContext context) { _context = context; }

        public async Task<IEnumerable<KhuyenMai>> GetAllAsync()
        {
            return await _context.KhuyenMais.OrderByDescending(x => x.NgayBatDau).ToListAsync();
        }

        public async Task<KhuyenMai> GetByIdAsync(int id)
        {
            return await _context.KhuyenMais.FindAsync(id);
        }

        public async Task<KhuyenMai> GetByCodeAsync(string code)
        {
            return await _context.KhuyenMais.FirstOrDefaultAsync(x => x.MaCodeKM == code);
        }

        public async Task<KhuyenMai> AddAsync(KhuyenMai entity)
        {
            await _context.KhuyenMais.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(KhuyenMai entity)
        {
            _context.KhuyenMais.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _context.KhuyenMais.FindAsync(id);
            if (item != null)
            {
                _context.KhuyenMais.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}