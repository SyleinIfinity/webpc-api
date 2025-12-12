using WEBPC_API.Models.Entities;

namespace WEBPC_API.Repositories.Interfaces
{
    public interface IDanhMucRepository
    {
        // Lấy tất cả danh mục (bao gồm thông tin cha - con)
        Task<List<DanhMuc>> GetAllAsync();

        // Lấy danh mục theo ID
        Task<DanhMuc?> GetByIdAsync(int id);

        // Tạo mới
        Task<DanhMuc> CreateAsync(DanhMuc danhMuc);

        // Cập nhật
        Task<DanhMuc> UpdateAsync(DanhMuc danhMuc);

        // Xóa
        Task DeleteAsync(int id);
    }
}