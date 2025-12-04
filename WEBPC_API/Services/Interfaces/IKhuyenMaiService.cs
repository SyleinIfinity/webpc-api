using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;

namespace WEBPC_API.Services.Interfaces
{
    public interface IKhuyenMaiService
    {
        // CRUD KhuyenMai
        Task<IEnumerable<KhuyenMaiResponse>> GetAllAsync();
        Task<KhuyenMaiResponse> GetByIdAsync(int id);
        Task<KhuyenMaiResponse> CreateAsync(CreateKhuyenMaiRequest request);
        Task<bool> UpdateAsync(int id, UpdateKhuyenMaiRequest request);
        Task<bool> DeleteAsync(int id);

        // Actions KhuyenMaiKhachHang (Gộp vào đây hoặc tách riêng Service tùy sở thích, tôi sẽ tách riêng cho rõ ràng)
    }
}