using WEBPC_API.Models.Entities;

namespace WEBPC_API.Repositories.Interfaces
{
    public interface IThongSoKyThuatRepository
    {
        // Lấy tất cả thông số của 1 sản phẩm
        Task<IEnumerable<ThongSoKyThuat>> GetByProductIdAsync(int maSanPham);

        // Lấy chi tiết 1 thông số
        Task<ThongSoKyThuat> GetByIdAsync(int id);

        // Thêm mới
        Task<ThongSoKyThuat> AddAsync(ThongSoKyThuat thongSo);

        // Cập nhật
        Task UpdateAsync(ThongSoKyThuat thongSo);

        // Xóa
        Task DeleteAsync(int id);
    }
}