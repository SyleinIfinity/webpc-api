using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;

namespace WEBPC_API.Repositories.Implements
{
    public class ThongSoKyThuatRepository : IThongSoKyThuatRepository
    {
        private readonly DataContext _context;

        public ThongSoKyThuatRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ThongSoKyThuat>> GetByProductIdAsync(int maSanPham)
        {
            return await _context.ThongSoKyThuats
                                 .Where(x => x.MaSanPham == maSanPham)
                                 .ToListAsync();
        }

        public async Task<ThongSoKyThuat> GetByIdAsync(int id)
        {
            return await _context.ThongSoKyThuats.FindAsync(id);
        }

        public async Task<ThongSoKyThuat> AddAsync(ThongSoKyThuat thongSo)
        {
            await _context.ThongSoKyThuats.AddAsync(thongSo);
            await _context.SaveChangesAsync();
            return thongSo;
        }

        public async Task UpdateAsync(ThongSoKyThuat thongSo)
        {
            _context.ThongSoKyThuats.Update(thongSo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _context.ThongSoKyThuats.FindAsync(id);
            if (item != null)
            {
                _context.ThongSoKyThuats.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}