using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;

namespace WEBPC_API.Repositories.Implements
{
    public class DonHangRepository : IDonHangRepository
    {
        private readonly DataContext _context;

        public DonHangRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<DonHang?> GetByIdAsync(int id)
        {
            return await _context.DonHang
                .Include(d => d.ChiTietDonHangs) // Kèm chi tiết để xem cho rõ
                .FirstOrDefaultAsync(d => d.maDonHang == id);
        }

        public async Task<GiaoDichThanhToan?> GetTransactionByOrderIdAsync(int orderId)
        {
            return await _context.GiaoDichThanhToan
                .Where(x => x.maDonHang == orderId)
                .OrderByDescending(x => x.ngayTao)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(DonHang donHang)
        {
            _context.DonHang.Update(donHang);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // --- PHẦN BỔ SUNG ---
        public async Task<IEnumerable<DonHang>> GetAllAsync()
        {
            // Lấy danh sách đơn, sắp xếp mới nhất lên đầu
            return await _context.DonHang
                .Include(d => d.KhachHang) // Kèm thông tin khách
                .OrderByDescending(d => d.ngayDat)
                .ToListAsync();
        }

        public async Task<DonHang> AddAsync(DonHang donHang)
        {
            _context.DonHang.Add(donHang);
            await _context.SaveChangesAsync();
            return donHang;
        }

        public async Task<IEnumerable<DonHang>> GetByKhachHangIdAsync(int maKhachHang)
        {
            return await _context.DonHang
                .Include(d => d.ChiTietDonHangs) // Kèm chi tiết để hiển thị sản phẩm
                    .ThenInclude(ct => ct.SanPham) // Kèm tên sản phẩm
                .Where(d => d.maKhachHang == maKhachHang) // Lọc theo khách
                .OrderByDescending(d => d.ngayDat) // Đơn mới nhất lên đầu
                .ToListAsync();
        }
    }
}