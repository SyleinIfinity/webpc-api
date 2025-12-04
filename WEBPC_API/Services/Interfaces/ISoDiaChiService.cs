using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Helpers;

namespace WEBPC_API.Services.Interfaces
{
    public interface ISoDiaChiService
    {
        Task<IEnumerable<SoDiaChiResponse>> GetByKhachHangIdAsync(int maKhachHang);
        Task<SoDiaChiResponse> GetByIdAsync(int id);
        Task<SoDiaChiResponse> CreateAsync(CreateSoDiaChiRequest request);
        Task<bool> UpdateAsync(int id, UpdateSoDiaChiRequest request);
        Task<bool> DeleteAsync(int id);

        // Các hàm phục vụ lấy dữ liệu hành chính cho Frontend
        Task<List<LocationHelper.LocationData>> GetProvincesAsync();
        Task<List<LocationHelper.LocationData>> GetDistrictsAsync(string provinceId);
        Task<List<LocationHelper.LocationData>> GetWardsAsync(string districtId);
    }
}