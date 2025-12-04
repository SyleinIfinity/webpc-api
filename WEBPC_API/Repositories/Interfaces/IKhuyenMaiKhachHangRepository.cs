using WEBPC_API.Models.Entities;

namespace WEBPC_API.Repositories.Interfaces
{
    public interface IKhuyenMaiKhachHangRepository
    {
        Task<IEnumerable<KhuyenMaiKhachHang>> GetByKhachHangIdAsync(int maKhachHang);
        Task<KhuyenMaiKhachHang> GetByIdAsync(int id);
        Task<bool> ExistsAsync(int maKhuyenMai, int maKhachHang); // Check xem đã thu thập chưa
        Task<KhuyenMaiKhachHang> AddAsync(KhuyenMaiKhachHang entity);
        Task DeleteAsync(int id);
    }
}