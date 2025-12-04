using System.Collections.Generic;
using System.Threading.Tasks;
using WEBPC_API.Models.Entities;

namespace WEBPC_API.Repositories.Interfaces
{
    public interface ITaiKhoanRepository
    {
        Task<List<TaiKhoan>> GetAllAsync();
        Task<TaiKhoan?> GetByIdAsync(int id);
        Task<TaiKhoan?> GetByUsernameAsync(string username); // Để login hoặc check trùng
        Task<TaiKhoan?> GetByEmailAsync(string email);       // Để check trùng
        Task AddAsync(TaiKhoan taiKhoan);
        Task UpdateAsync(TaiKhoan taiKhoan);
        Task DeleteAsync(int id);

        // Hàm kiểm tra nhanh sự tồn tại
        Task<bool> IsUsernameExistsAsync(string username);
        Task<bool> IsEmailExistsAsync(string email);
    }
}