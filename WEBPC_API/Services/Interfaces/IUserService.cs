using System.Collections.Generic;
using System.Threading.Tasks;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;

namespace WEBPC_API.Services.Interfaces
{
    public interface IUserService
    {
        // --- CÁC HÀM CŨ (GIỮ NGUYÊN) ---
        // 1. Quản lý Vai trò
        Task<List<VaiTroResponse>> GetAllVaiTros();
        Task<bool> CreateVaiTro(VaiTroRequest req);
        Task<bool> DeleteVaiTro(int id);

        // 2. Quản lý Nhân viên
        Task<List<NhanVienResponse>> GetAllNhanViens();
        Task<NhanVienResponse?> GetNhanVienById(int id);
        Task<(bool IsSuccess, string Message)> CreateNhanVien(NhanVienRequest req);
        Task<bool> UpdateNhanVien(int id, NhanVienRequest req);
        Task<bool> DeleteNhanVien(int id);

        // 3. Quản lý Khách hàng
        Task<List<KhachHangResponse>> GetAllKhachHangs();
        Task<(bool IsSuccess, string Message)> CreateKhachHang(KhachHangRequest req);
        Task<bool> UpdateKhachHang(int id, UpdateKhachHangRequest req);

        // 4. Quản lý Tài khoản (Cũ)
        Task<List<TaiKhoanResponse>> GetAllTaiKhoans();
        Task<bool> ChangePassword(int id, string newPass);

        // --- BỔ SUNG HÀM ĐĂNG NHẬP Ở ĐÂY ---
        Task<LoginResponse?> Login(LoginRequest request);

        Task<string> UpdateAvatarAsync(int id, IFormFile file); // Trả về URL ảnh mới
        Task<bool> UpdateStatusAsync(int id, string trangThaiMoi);
    }
}