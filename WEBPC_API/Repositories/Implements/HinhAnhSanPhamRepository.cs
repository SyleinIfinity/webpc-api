using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Models.Entities;
using WEBPC_API.Models.Interfaces;

namespace WEBPC_API.Models.Repositories
{
    public class HinhAnhSanPhamRepository : IHinhAnhSanPhamRepository
    {
        private readonly DataContext _context;

        public HinhAnhSanPhamRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<HinhAnhSanPham> GetByIdAsync(int id)
        {
            return await _context.HinhAnhSanPhams.FindAsync(id);
        }

        public async Task<IEnumerable<HinhAnhSanPham>> GetByProductIdAsync(int productId)
        {
            return await _context.HinhAnhSanPhams
                                 .Where(x => x.MaSanPham == productId)
                                 .ToListAsync();
        }

        public async Task AddAsync(HinhAnhSanPham hinhAnh)
        {
            _context.HinhAnhSanPhams.Add(hinhAnh);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(HinhAnhSanPham hinhAnh)
        {
            _context.HinhAnhSanPhams.Remove(hinhAnh);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(HinhAnhSanPham image)
        {
            _context.HinhAnhSanPhams.Update(image);
            await _context.SaveChangesAsync();
        }
    }
}