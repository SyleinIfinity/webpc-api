using WEBPC_API.Models.Entities;

namespace WEBPC_API.Models.Interfaces
{
    public interface ISanPhamRepository
    {
        Task<IEnumerable<SanPham>> GetAllAsync();
        Task<SanPham> GetByIdAsync(int id);
        Task<int> AddAsync(SanPham sanPham);
        Task UpdateAsync(SanPham sanPham);
        Task DeleteAsync(int id);

        // Các hàm hỗ trợ hình ảnh
        Task AddImagesAsync(List<HinhAnhSanPham> images);
        Task DeleteImagesAsync(List<string> publicIds); // Xóa ảnh trong DB dựa trên PublicId

        Task<IEnumerable<SanPham>> GetByCategoryIdAsync(int maDanhMuc);
    }
}