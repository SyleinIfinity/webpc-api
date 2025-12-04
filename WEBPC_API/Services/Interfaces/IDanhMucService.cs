using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;

namespace WEBPC_API.Services.Interfaces
{
    public interface IDanhMucService
    {
        Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync();
        Task<CategoryResponse> GetCategoryByIdAsync(int id);
        Task<bool> CreateCategoryAsync(CreateCategoryRequest request);
        Task<bool> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
        Task<string> DeleteCategoryAsync(int id); // Trả về string để báo lỗi chi tiết
    }
}