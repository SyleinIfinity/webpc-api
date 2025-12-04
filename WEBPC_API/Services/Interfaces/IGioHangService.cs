using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;

namespace WEBPC_API.Services.Interfaces
{
    public interface IGioHangService
    {
        Task<GioHangResponse> GetGioHangByKhachHangAsync(int maKhachHang);
        Task<GioHangResponse> AddToCartAsync(AddToCartRequest request);
        Task<GioHangResponse> UpdateCartItemAsync(UpdateCartItemRequest request);
        Task<GioHangResponse> RemoveFromCartAsync(int maKhachHang, int maSanPham);
        Task<bool> ClearCartAsync(int maKhachHang);
    }
}