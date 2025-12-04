using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;

namespace WEBPC_API.Services.Interfaces
{
    public interface IPhieuNhapService
    {
        Task<List<PhieuNhapResponse>> GetAllPhieuNhapAsync();
        Task<PhieuNhapResponse> GetPhieuNhapByIdAsync(int id);
        Task<PhieuNhapResponse> CreatePhieuNhapAsync(CreatePhieuNhapRequest request);
        Task<PhieuNhapResponse> UpdatePhieuNhapAsync(int id, UpdatePhieuNhapRequest request);
        Task<bool> DeletePhieuNhapAsync(int id);
    }
}