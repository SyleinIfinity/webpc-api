using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Services.Business
{
    public class ThongSoKyThuatService : IThongSoKyThuatService
    {
        private readonly IThongSoKyThuatRepository _repo;

        public ThongSoKyThuatService(IThongSoKyThuatRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ThongSoKyThuatResponse>> GetByProductIdAsync(int maSanPham)
        {
            var list = await _repo.GetByProductIdAsync(maSanPham);
            return list.Select(x => new ThongSoKyThuatResponse
            {
                MaThongSo = x.MaThongSo,
                MaSanPham = x.MaSanPham,
                TenThongSo = x.TenThongSo,
                GiaTri = x.GiaTri
            });
        }

        public async Task<ThongSoKyThuatResponse> CreateAsync(CreateThongSoRequest request)
        {
            var newThongSo = new ThongSoKyThuat
            {
                MaSanPham = request.MaSanPham,
                TenThongSo = request.TenThongSo,
                GiaTri = request.GiaTri
            };

            var created = await _repo.AddAsync(newThongSo);

            return new ThongSoKyThuatResponse
            {
                MaThongSo = created.MaThongSo,
                MaSanPham = created.MaSanPham,
                TenThongSo = created.TenThongSo,
                GiaTri = created.GiaTri
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateThongSoRequest request)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            existing.TenThongSo = request.TenThongSo;
            existing.GiaTri = request.GiaTri;

            await _repo.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            await _repo.DeleteAsync(id);
            return true;
        }
    }
}