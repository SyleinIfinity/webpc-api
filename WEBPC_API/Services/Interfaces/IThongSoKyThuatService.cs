using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;

namespace WEBPC_API.Services.Interfaces
{
    public interface IThongSoKyThuatService
    {
        Task<IEnumerable<ThongSoKyThuatResponse>> GetByProductIdAsync(int maSanPham);
        Task<ThongSoKyThuatResponse> CreateAsync(CreateThongSoRequest request);
        Task<bool> UpdateAsync(int id, UpdateThongSoRequest request);
        Task<bool> DeleteAsync(int id);
    }
}