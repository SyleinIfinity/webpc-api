using WEBPC_API.Models.Entities;

namespace WEBPC_API.Repositories.Interfaces
{
    public interface INhatKyHoatDongRepository
    {
        Task AddLogAsync(string hanhDong, string moTa, int? maNhanVien);
    }
}