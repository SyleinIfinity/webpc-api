using WEBPC_API.Models.Entities;

namespace WEBPC_API.Repositories.Interfaces
{
    public interface IPhieuNhapRepository
    {
        Task<List<PhieuNhap>> GetAllAsync();
        Task<PhieuNhap?> GetByIdAsync(int id);
        Task<PhieuNhap?> GetByCodeAsync(string code);
        Task<PhieuNhap> CreateAsync(PhieuNhap phieuNhap);
        Task<PhieuNhap> UpdateAsync(PhieuNhap phieuNhap);
        Task DeleteAsync(int id);
    }
}