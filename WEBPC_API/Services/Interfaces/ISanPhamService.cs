using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;

namespace WEBPC_API.Services.Interfaces
{
    public interface ISanPhamService
    {
        Task<IEnumerable<ProductResponse>> GetAllProductsAsync();
        Task<ProductResponse> GetProductByIdAsync(int id);
        Task<bool> CreateProductAsync(CreateProductRequest request);
        Task<bool> UpdateProductAsync(int id, UpdateProductRequest request);
        Task<bool> DeleteProductAsync(int id);
        Task<IEnumerable<ProductResponse>> GetProductsByCategoryIdAsync(int maDanhMuc);
    }
}