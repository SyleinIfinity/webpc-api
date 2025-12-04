using WEBPC_API.Helpers;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Services.Business
{
    public class SoDiaChiService : ISoDiaChiService
    {
        private readonly ISoDiaChiRepository _repo;
        private readonly LocationHelper _locationHelper;

        public SoDiaChiService(ISoDiaChiRepository repo, LocationHelper locationHelper)
        {
            _repo = repo;
            _locationHelper = locationHelper;
        }

        public async Task<IEnumerable<SoDiaChiResponse>> GetByKhachHangIdAsync(int maKhachHang)
        {
            var list = await _repo.GetByKhachHangIdAsync(maKhachHang);
            return list.Select(MapToResponse);
        }

        public async Task<SoDiaChiResponse> GetByIdAsync(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            return item == null ? null : MapToResponse(item);
        }

        public async Task<SoDiaChiResponse> CreateAsync(CreateSoDiaChiRequest request)
        {
            // 1. Gọi Helper lấy tên địa danh từ ID
            var names = await _locationHelper.GetAddressNamesAsync(request.TinhThanhId, request.QuanHuyenId, request.PhuongXaId);

            if (names.Tinh == null || names.Huyen == null || names.Xa == null)
            {
                throw new Exception("Thông tin Tỉnh/Huyện/Xã không hợp lệ.");
            }

            var entity = new SoDiaChi
            {
                MaKhachHang = request.MaKhachHang,
                DiaChiCuThe = request.DiaChiCuThe,
                TinhThanhId = request.TinhThanhId,
                QuanHuyenId = request.QuanHuyenId,
                PhuongXaId = request.PhuongXaId,
                TenTinhThanh = names.Tinh,
                TenQuanHuyen = names.Huyen,
                TenPhuongXa = names.Xa,
                MacDinh = request.MacDinh
            };

            var created = await _repo.AddAsync(entity);

            // Nếu địa chỉ mới là mặc định, reset các cái cũ
            if (created.MacDinh)
            {
                await _repo.ResetDefaultAddressAsync(created.MaKhachHang, created.MaSoDiaChi);
            }

            return MapToResponse(created);
        }

        public async Task<bool> UpdateAsync(int id, UpdateSoDiaChiRequest request)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            // Nếu có thay đổi địa giới hành chính -> Cần lấy lại tên
            bool locationChanged = false;
            if (!string.IsNullOrEmpty(request.TinhThanhId) && request.TinhThanhId != existing.TinhThanhId) locationChanged = true;
            if (!string.IsNullOrEmpty(request.QuanHuyenId) && request.QuanHuyenId != existing.QuanHuyenId) locationChanged = true;
            if (!string.IsNullOrEmpty(request.PhuongXaId) && request.PhuongXaId != existing.PhuongXaId) locationChanged = true;

            if (locationChanged)
            {
                // Logic: Phải gửi đủ cả 3 ID mới hoặc dùng lại ID cũ nếu request không gửi
                string pId = request.TinhThanhId ?? existing.TinhThanhId;
                string dId = request.QuanHuyenId ?? existing.QuanHuyenId;
                string wId = request.PhuongXaId ?? existing.PhuongXaId;

                var names = await _locationHelper.GetAddressNamesAsync(pId, dId, wId);
                if (names.Tinh == null || names.Huyen == null || names.Xa == null)
                    throw new Exception("Thông tin địa chính không hợp lệ.");

                existing.TinhThanhId = pId;
                existing.QuanHuyenId = dId;
                existing.PhuongXaId = wId;
                existing.TenTinhThanh = names.Tinh;
                existing.TenQuanHuyen = names.Huyen;
                existing.TenPhuongXa = names.Xa;
            }

            if (!string.IsNullOrEmpty(request.DiaChiCuThe)) existing.DiaChiCuThe = request.DiaChiCuThe;

            if (request.MacDinh.HasValue)
            {
                existing.MacDinh = request.MacDinh.Value;
                if (existing.MacDinh)
                {
                    await _repo.ResetDefaultAddressAsync(existing.MaKhachHang, existing.MaSoDiaChi);
                }
            }

            await _repo.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return false;
            await _repo.DeleteAsync(id);
            return true;
        }

        // Proxy methods
        public Task<List<LocationHelper.LocationData>> GetProvincesAsync() => _locationHelper.GetProvincesAsync();
        public Task<List<LocationHelper.LocationData>> GetDistrictsAsync(string pId) => _locationHelper.GetDistrictsAsync(pId);
        public Task<List<LocationHelper.LocationData>> GetWardsAsync(string dId) => _locationHelper.GetWardsAsync(dId);

        private SoDiaChiResponse MapToResponse(SoDiaChi s)
        {
            return new SoDiaChiResponse
            {
                MaSoDiaChi = s.MaSoDiaChi,
                MaKhachHang = s.MaKhachHang,
                DiaChiCuThe = s.DiaChiCuThe,
                TinhThanhId = s.TinhThanhId,
                TenTinhThanh = s.TenTinhThanh,
                QuanHuyenId = s.QuanHuyenId,
                TenQuanHuyen = s.TenQuanHuyen,
                PhuongXaId = s.PhuongXaId,
                TenPhuongXa = s.TenPhuongXa,
                MacDinh = s.MacDinh
            };
        }
    }
}