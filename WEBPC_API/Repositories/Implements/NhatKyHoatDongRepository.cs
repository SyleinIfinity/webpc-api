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

        public async Task AddLogAsync(string hanhDong, string moTa, int? maNhanVien)
        {
            var log = new NhatKyHoatDong
            {
                HanhDong = hanhDong,
                MoTa = moTa,
                MaNhanVien = maNhanVien,
                ThoiGian = DateTime.Now
            };

            _context.NhatKyHoatDong.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}