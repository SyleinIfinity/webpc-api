using System.Collections.Generic;
using System.Threading.Tasks;
using WEBPC_API.Models.Entities;

namespace WEBPC_API.Repositories.Interfaces
{
    public interface IKhachHangRepository
    {
        Task<List<KhachHang>> GetAllAsync();
        Task<KhachHang?> GetByIdAsync(int id);
        Task<KhachHang?> GetByPhoneAsync(string phone); // Tìm khách theo SĐT
        Task AddAsync(KhachHang khachHang);
        Task UpdateAsync(KhachHang khachHang);
        Task DeleteAsync(int id);
    }
}