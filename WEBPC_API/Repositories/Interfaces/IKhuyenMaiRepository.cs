using WEBPC_API.Models.Entities;

namespace WEBPC_API.Repositories.Interfaces
{
    public interface IKhuyenMaiRepository
    {
        Task<IEnumerable<KhuyenMai>> GetAllAsync();
        Task<KhuyenMai> GetByIdAsync(int id);
        Task<KhuyenMai> GetByCodeAsync(string code);
        Task<KhuyenMai> AddAsync(KhuyenMai entity);
        Task UpdateAsync(KhuyenMai entity);
        Task DeleteAsync(int id);
    }
}