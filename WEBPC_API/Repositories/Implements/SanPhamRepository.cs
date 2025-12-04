using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Data;
using WEBPC_API.Models.Entities;
using WEBPC_API.Models.Interfaces;

namespace WEBPC_API.Models.Repositories
{
    public class SanPhamRepository : ISanPhamRepository
    {
        private readonly DataContext _context;

        public SanPhamRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SanPham>> GetAllAsync()
        {
            // Include để lấy kèm danh sách hình ảnh khi query sản phẩm
            return await _context.SanPhams
                                     .Include(p => p.HinhAnhs)
                                     .Include(p => p.DanhMuc) // <--- Thêm dòng này
                                     .ToListAsync();
        }

        public async Task<SanPham> GetByIdAsync(int id)
        {
            return await _context.SanPhams
                                     .Include(p => p.HinhAnhs)
                                     .Include(p => p.DanhMuc) // <--- Thêm dòng này
                                     .FirstOrDefaultAsync(p => p.MaSanPham == id);
        }

        public async Task<int> AddAsync(SanPham sanPham)
        {
            _context.SanPhams.Add(sanPham);
            await _context.SaveChangesAsync();
            return sanPham.MaSanPham; // Trả về ID vừa sinh ra
        }

        public async Task UpdateAsync(SanPham sanPham)
        {
            _context.SanPhams.Update(sanPham);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp != null)
            {
                _context.SanPhams.Remove(sp);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddImagesAsync(List<HinhAnhSanPham> images)
        {
            await _context.Set<HinhAnhSanPham>().AddRangeAsync(images);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteImagesAsync(List<string> publicIds)
        {
            var images = await _context.Set<HinhAnhSanPham>()
                                       .Where(x => publicIds.Contains(x.PublicId))
                                       .ToListAsync();
            _context.Set<HinhAnhSanPham>().RemoveRange(images);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SanPham>> GetByCategoryIdAsync(int maDanhMuc)
        {
            return await _context.SanPhams
                                 .Include(p => p.HinhAnhs) // Lấy kèm hình ảnh
                                 .Include(p => p.DanhMuc)  // Lấy kèm tên danh mục
                                 .Where(p => p.MaDanhMuc == maDanhMuc) // Điều kiện lọc
                                 .ToListAsync();
        }
    }
}