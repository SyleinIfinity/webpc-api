using WEBPC_API.Models.Entities;

namespace WEBPC_API.Models.Interfaces
{
    public interface IHinhAnhSanPhamRepository
    {
        Task<HinhAnhSanPham> GetByIdAsync(int id);
        Task<IEnumerable<HinhAnhSanPham>> GetByProductIdAsync(int productId);
        Task AddAsync(HinhAnhSanPham hinhAnh);
        Task DeleteAsync(HinhAnhSanPham hinhAnh);
    }
}