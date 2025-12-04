using WEBPC_API.Models.Entities;

namespace WEBPC_API.Repositories.Interfaces
{
    public interface ISoDiaChiRepository
    {
        Task<IEnumerable<SoDiaChi>> GetByKhachHangIdAsync(int maKhachHang);
        Task<SoDiaChi> GetByIdAsync(int id);
        Task<SoDiaChi> AddAsync(SoDiaChi entity);
        Task UpdateAsync(SoDiaChi entity);
        Task DeleteAsync(int id);

        // Hàm hỗ trợ reset các địa chỉ khác về không mặc định
        Task ResetDefaultAddressAsync(int maKhachHang, int excludeId);
    }
}