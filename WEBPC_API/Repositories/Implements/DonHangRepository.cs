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

        public async Task<DonHang> AddAsync(DonHang donHang)
        {
            // Khi Add donHang, EF Core sẽ tự động Add luôn các ChiTietDonHang bên trong
            // nhờ cơ chế Navigation Property
            await _context.DonHang.AddAsync(donHang);
            await _context.SaveChangesAsync();
            return donHang;
        }

        public async Task<DonHang?> GetByIdAsync(int id)
        {
            return await _context.DonHang
                .Include(d => d.ChiTietDonHangs) // Kèm chi tiết đơn
                .ThenInclude(ct => ct.SanPham)   // Kèm thông tin sản phẩm
                .FirstOrDefaultAsync(d => d.maDonHang == id);
        }

        public async Task<DonHang?> GetByCodeAsync(string maCode)
        {
            return await _context.DonHang
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.maCodeDonHang == maCode);
        }

        public async Task<List<DonHang>> GetByKhachHangIdAsync(int maKhachHang)
        {
            return await _context.DonHang
                .Where(d => d.maKhachHang == maKhachHang)
                .OrderByDescending(d => d.ngayDat) // Đơn mới nhất lên đầu
                .ToListAsync();
        }

        public async Task UpdateAsync(DonHang donHang)
        {
            _context.DonHang.Update(donHang);
            await _context.SaveChangesAsync();
        }
        public async Task<List<DonHang>> GetAllAsync()
        {
            return await _context.DonHang
                .Include(d => d.KhachHang) // Kèm thông tin khách để hiển thị tên
                .OrderByDescending(d => d.ngayDat)
                .ToListAsync();
        }

        // Trong DonHangRepository.cs
        public async Task<GiaoDichThanhToan?> GetTransactionByOrderIdAsync(int orderId)
        {
            return await _context.GiaoDichThanhToan
                .FirstOrDefaultAsync(x => x.maDonHang == orderId);
        }

        // 1. GET ALL
        public async Task<List<DonHang>> GetAllOrdersFullAsync()
        {
            return await _context.DonHang
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                        .ThenInclude(sp => sp.HinhAnhs) // Lấy ảnh để hiển thị
                .OrderByDescending(d => d.ngayDat) // Đơn mới nhất lên đầu
                .ToListAsync();
        }

        // 2. GET BY ID
        public async Task<DonHang?> GetOrderByIdFullAsync(int id)
        {
            return await _context.DonHang
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                        .ThenInclude(sp => sp.HinhAnhs)
                .FirstOrDefaultAsync(d => d.maDonHang == id);
        }

        // 3. GET BY CUSTOMER ID
        public async Task<List<DonHang>> GetOrdersByCustomerIdAsync(int maKhachHang)
        {
            return await _context.DonHang
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                        .ThenInclude(sp => sp.HinhAnhs)
                .Where(d => d.maKhachHang == maKhachHang)
                .OrderByDescending(d => d.ngayDat)
                .ToListAsync();
        }
    }
}