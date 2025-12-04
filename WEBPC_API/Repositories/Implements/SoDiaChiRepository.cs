using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;

namespace WEBPC_API.Repositories.Implements
{
    public class SoDiaChiRepository : ISoDiaChiRepository
    {
        private readonly DataContext _context;

        public SoDiaChiRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SoDiaChi>> GetByKhachHangIdAsync(int maKhachHang)
        {
            return await _context.SoDiaChis
                                 .Where(x => x.MaKhachHang == maKhachHang)
                                 .OrderByDescending(x => x.MacDinh) // Mặc định lên đầu
                                 .ToListAsync();
        }

        public async Task<SoDiaChi> GetByIdAsync(int id)
        {
            return await _context.SoDiaChis.FindAsync(id);
        }

        public async Task<SoDiaChi> AddAsync(SoDiaChi entity)
        {
            await _context.SoDiaChis.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(SoDiaChi entity)
        {
            _context.SoDiaChis.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _context.SoDiaChis.FindAsync(id);
            if (item != null)
            {
                _context.SoDiaChis.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ResetDefaultAddressAsync(int maKhachHang, int excludeId)
        {
            var list = await _context.SoDiaChis
                .Where(x => x.MaKhachHang == maKhachHang && x.MaSoDiaChi != excludeId && x.MacDinh)
                .ToListAsync();

            if (list.Any())
            {
                foreach (var item in list)
                {
                    item.MacDinh = false;
                }
                _context.SoDiaChis.UpdateRange(list);
                await _context.SaveChangesAsync();
            }
        }
    }
}