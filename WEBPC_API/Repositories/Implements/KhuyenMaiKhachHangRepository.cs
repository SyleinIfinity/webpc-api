using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;

namespace WEBPC_API.Repositories.Implements
{
    public class KhuyenMaiKhachHangRepository : IKhuyenMaiKhachHangRepository
    {
        private readonly DataContext _context;
        public KhuyenMaiKhachHangRepository(DataContext context) { _context = context; }

        public async Task<IEnumerable<KhuyenMaiKhachHang>> GetByKhachHangIdAsync(int maKhachHang)
        {
            // Include bảng KhuyenMai để lấy thông tin chi tiết
            return await _context.KhuyenMaiKhachHangs
                                 .Include(x => x.KhuyenMai)
                                 .Where(x => x.MaKhachHang == maKhachHang)
                                 .ToListAsync();
        }

        public async Task<KhuyenMaiKhachHang> GetByIdAsync(int id)
        {
            return await _context.KhuyenMaiKhachHangs.FindAsync(id);
        }

        public async Task<bool> ExistsAsync(int maKhuyenMai, int maKhachHang)
        {
            return await _context.KhuyenMaiKhachHangs.AnyAsync(x => x.MaKhuyenMai == maKhuyenMai && x.MaKhachHang == maKhachHang);
        }

        public async Task<KhuyenMaiKhachHang> AddAsync(KhuyenMaiKhachHang entity)
        {
            await _context.KhuyenMaiKhachHangs.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _context.KhuyenMaiKhachHangs.FindAsync(id);
            if (item != null)
            {
                _context.KhuyenMaiKhachHangs.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}