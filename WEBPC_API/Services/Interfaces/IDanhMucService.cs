using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;

namespace WEBPC_API.Services.Interfaces
{
    public interface IDanhMucService
    {
        // Lấy tất cả danh mục
        Task<List<CategoryResponse>> GetAllCategoriesAsync();

        // Lấy chi tiết 1 danh mục
        Task<CategoryResponse> GetCategoryByIdAsync(int id);

        // Tạo mới danh mục
        Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request);

        // Cập nhật danh mục
        Task<CategoryResponse> UpdateCategoryAsync(int id, UpdateCategoryRequest request);

        // Xóa danh mục
        Task<bool> DeleteCategoryAsync(int id);
    }
}