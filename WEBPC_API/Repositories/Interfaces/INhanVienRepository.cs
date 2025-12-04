using System.Collections.Generic;
using System.Threading.Tasks;
using WEBPC_API.Models.Entities;

namespace WEBPC_API.Repositories.Interfaces
{
    public interface INhanVienRepository
    {
        Task<List<NhanVien>> GetAllAsync();
        Task<NhanVien?> GetByIdAsync(int id);
        Task<NhanVien?> GetByCodeAsync(string code); // Tìm theo mã NV (VD: NV1234)
        Task AddAsync(NhanVien nhanVien);
        Task UpdateAsync(NhanVien nhanVien);
        Task DeleteAsync(int id);
    }
}