using System.Collections.Generic;
using System.Threading.Tasks;
using WEBPC_API.Models.Entities;

namespace WEBPC_API.Repositories.Interfaces
{
    public interface IVaiTroRepository
    {
        Task<List<VaiTro>> GetAllAsync();
        Task<VaiTro?> GetByIdAsync(int id);
        Task AddAsync(VaiTro vaiTro);
        Task UpdateAsync(VaiTro vaiTro);
        Task DeleteAsync(int id);
    }
}