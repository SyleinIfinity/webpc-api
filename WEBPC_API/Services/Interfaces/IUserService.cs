using System.Collections.Generic;
using System.Threading.Tasks;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;

namespace WEBPC_API.Services.Interfaces
{
    public interface IUserService
    {
        // --- QUẢN LÝ VAI TRÒ ---
        Task<List<VaiTroResponse>> GetAllVaiTros();
        Task<bool> CreateVaiTro(VaiTroRequest req);
        Task<bool> DeleteVaiTro(int id);

        // --- QUẢN LÝ NHÂN VIÊN ---
        Task<List<NhanVienResponse>> GetAllNhanViens();
        Task<NhanVienResponse?> GetNhanVienById(int id);

        // Trả về Tuple (Thành công/Thất bại, Thông báo lỗi/thành công)
        Task<(bool IsSuccess, string Message)> CreateNhanVien(NhanVienRequest req);

        Task<bool> UpdateNhanVien(int id, NhanVienRequest req);
        Task<bool> DeleteNhanVien(int id);

        // --- QUẢN LÝ KHÁCH HÀNG ---
        Task<List<KhachHangResponse>> GetAllKhachHangs();
        Task<(bool IsSuccess, string Message)> CreateKhachHang(KhachHangRequest req);
        Task<bool> UpdateKhachHang(int id, UpdateKhachHangRequest req);

        // --- QUẢN LÝ TÀI KHOẢN ---
        Task<List<TaiKhoanResponse>> GetAllTaiKhoans();
        Task<bool> ChangePassword(int id, string newPass);
    }
}