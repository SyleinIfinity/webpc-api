using WEBPC_API.Models.Entities;

namespace WEBPC_API.Repositories.Interfaces
{
    public interface INhatKyHoatDongRepository
    {
        Task AddLogAsync(NhatKyHoatDong log);
    }
}