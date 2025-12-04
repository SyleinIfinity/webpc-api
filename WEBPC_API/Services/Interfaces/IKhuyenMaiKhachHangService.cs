using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;

namespace WEBPC_API.Services.Interfaces
{
    public interface IKhuyenMaiKhachHangService
    {
        Task<IEnumerable<KhuyenMaiKhachHangResponse>> GetByKhachHangIdAsync(int maKhachHang);
        Task<string> ThuThapKhuyenMaiAsync(ThuThapKhuyenMaiRequest request); // Trả về message
        Task<bool> XoaKhuyenMaiCuaKhachAsync(int id);
    }
}