using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;

namespace WEBPC_API.Repositories.Implements
{
    public class NhatKyHoatDongRepository : INhatKyHoatDongRepository
    {
        private readonly DataContext _context;

        public NhatKyHoatDongRepository(DataContext context)
        {
            _context = context;
        }

        // Đây là hàm em bị thiếu dẫn đến lỗi build
        public async Task AddLogAsync(NhatKyHoatDong log)
        {
            // Thêm log vào DbSet
            await _context.NhatKyHoatDong.AddAsync(log);

            // Lưu thay đổi xuống Database
            await _context.SaveChangesAsync();
        }
    }
}