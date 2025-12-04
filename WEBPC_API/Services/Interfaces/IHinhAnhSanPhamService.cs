using WEBPC_API.Models.DTOs.Responses;

namespace WEBPC_API.Services.Interfaces
{
    public interface IHinhAnhSanPhamService
    {
        Task<List<ImageResponse>> GetImagesByProductId(int productId);

        // Upload 1 ảnh cho sản phẩm đã tồn tại
        Task<ImageResponse> AddImageToProductAsync(int productId, IFormFile file);

        // Xóa 1 ảnh theo ID
        Task<bool> DeleteImageAsync(int imageId);
    }
}