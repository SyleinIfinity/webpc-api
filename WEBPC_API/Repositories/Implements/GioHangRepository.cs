using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;

namespace WEBPC_API.Repositories.Implements
{
    public class GioHangRepository : IGioHangRepository
    {
        private readonly DataContext _context;

        public GioHangRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<GioHang?> GetByKhachHangIdAsync(int maKhachHang)
        {
            return await _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                    .ThenInclude(ct => ct.SanPham)
                        .ThenInclude(sp => sp.HinhAnhs) // Để lấy ảnh hiển thị
                .FirstOrDefaultAsync(g => g.MaKhachHang == maKhachHang);
        }

        public async Task<GioHang> CreateAsync(GioHang gioHang)
        {
            _context.GioHangs.Add(gioHang);
            await _context.SaveChangesAsync();
            return gioHang;
        }

        public async Task AddOrUpdateItemAsync(ChiTietGioHang item)
        {
            if (item.MaChiTietGioHang == 0)
            {
                _context.ChiTietGioHangs.Add(item);
            }
            else
            {
                _context.ChiTietGioHangs.Update(item);
            }
            await _context.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(int maGioHang, int maSanPham)
        {
            var item = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(x => x.MaGioHang == maGioHang && x.MaSanPham == maSanPham);

            if (item != null)
            {
                _context.ChiTietGioHangs.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(int maGioHang)
        {
            var items = _context.ChiTietGioHangs.Where(x => x.MaGioHang == maGioHang);
            _context.ChiTietGioHangs.RemoveRange(items);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // Hàm RemoveCartItemsAsync
        public async Task RemoveCartItemsAsync(int maGioHang, List<int> listChiTietId)
        {
            if (listChiTietId == null || !listChiTietId.Any()) return;

            // [SỬA]: Xóa theo MaChiTietGioHang (Đây là Primary Key nên rất chính xác)
            var itemsToDelete = await _context.ChiTietGioHangs
                .Where(x => x.MaGioHang == maGioHang && listChiTietId.Contains(x.MaChiTietGioHang))
                .ToListAsync();

            if (itemsToDelete.Any())
            {
                _context.ChiTietGioHangs.RemoveRange(itemsToDelete);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<ChiTietGioHang>> GetSelectedItemsAsync(int maGioHang, List<int> listChiTietId)
        {
            if (listChiTietId == null || !listChiTietId.Any())
            {
                return new List<ChiTietGioHang>();
            }

            return await _context.ChiTietGioHangs
                .Include(ct => ct.SanPham)
                    .ThenInclude(sp => sp.HinhAnhs) // Include ảnh để hiển thị đẹp
                .Where(x => x.MaGioHang == maGioHang && listChiTietId.Contains(x.MaChiTietGioHang))
                .ToListAsync();
        }
    }
}