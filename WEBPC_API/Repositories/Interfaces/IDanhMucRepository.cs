using WEBPC_API.Models.Entities;

namespace WEBPC_API.Models.Interfaces
{
    public interface IDanhMucRepository
    {
        Task<IEnumerable<DanhMuc>> GetAllAsync();
        Task<DanhMuc> GetByIdAsync(int id);
        Task<int> AddAsync(DanhMuc danhMuc);
        Task UpdateAsync(DanhMuc danhMuc);
        Task DeleteAsync(int id);

        // Hàm kiểm tra trùng tên danh mục
        Task<bool> IsNameExistsAsync(string name);

        // Hàm kiểm tra xem danh mục có chứa sản phẩm không (để chặn xóa)
        Task<bool> HasProductsAsync(int id);
    }
}