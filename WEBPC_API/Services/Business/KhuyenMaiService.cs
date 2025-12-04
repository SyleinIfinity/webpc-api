using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Services.Business
{
    public class KhuyenMaiService : IKhuyenMaiService
    {
        private readonly IKhuyenMaiRepository _repo;

        public KhuyenMaiService(IKhuyenMaiRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<KhuyenMaiResponse>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToResponse);
        }

        public async Task<KhuyenMaiResponse> GetByIdAsync(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            return item == null ? null : MapToResponse(item);
        }

        public async Task<KhuyenMaiResponse> CreateAsync(CreateKhuyenMaiRequest request)
        {
            // Kiểm tra Logic ngày tháng
            if (request.NgayKetThuc < request.NgayBatDau)
                throw new Exception("Ngày kết thúc phải lớn hơn ngày bắt đầu.");

            // Kiểm tra trùng mã code
            var exists = await _repo.GetByCodeAsync(request.MaCodeKM);
            if (exists != null)
                throw new Exception("Mã khuyến mãi này đã tồn tại.");

            var km = new KhuyenMai
            {
                MaCodeKM = request.MaCodeKM,
                TenChuongTrinh = request.TenChuongTrinh,
                GiaTriGiam = request.GiaTriGiam,
                LoaiGiam = request.LoaiGiam,
                DonHangToiThieu = request.DonHangToiThieu,
                GiamToiDa = request.GiamToiDa,
                NgayBatDau = request.NgayBatDau,
                NgayKetThuc = request.NgayKetThuc,
                SoLuongConLai = request.SoLuongConLai
            };

            var created = await _repo.AddAsync(km);
            return MapToResponse(created);
        }

        public async Task<bool> UpdateAsync(int id, UpdateKhuyenMaiRequest request)
        {
            var km = await _repo.GetByIdAsync(id);
            if (km == null) return false;

            if (request.NgayKetThuc < request.NgayBatDau)
                throw new Exception("Ngày kết thúc phải lớn hơn ngày bắt đầu.");

            // Update fields
            km.TenChuongTrinh = request.TenChuongTrinh;
            km.GiaTriGiam = request.GiaTriGiam;
            km.LoaiGiam = request.LoaiGiam;
            km.DonHangToiThieu = request.DonHangToiThieu;
            km.GiamToiDa = request.GiamToiDa;
            km.NgayBatDau = request.NgayBatDau;
            km.NgayKetThuc = request.NgayKetThuc;
            km.SoLuongConLai = request.SoLuongConLai;
            // Không update MaCodeKM để đảm bảo tính toàn vẹn hoặc check unique nếu cho sửa

            await _repo.UpdateAsync(km);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var km = await _repo.GetByIdAsync(id);
            if (km == null) return false;
            await _repo.DeleteAsync(id);
            return true;
        }

        private KhuyenMaiResponse MapToResponse(KhuyenMai km)
        {
            return new KhuyenMaiResponse
            {
                MaKhuyenMai = km.MaKhuyenMai,
                MaCodeKM = km.MaCodeKM,
                TenChuongTrinh = km.TenChuongTrinh,
                GiaTriGiam = km.GiaTriGiam,
                LoaiGiam = km.LoaiGiam,
                DonHangToiThieu = km.DonHangToiThieu,
                GiamToiDa = km.GiamToiDa,
                NgayBatDau = km.NgayBatDau,
                NgayKetThuc = km.NgayKetThuc,
                SoLuongConLai = km.SoLuongConLai
            };
        }
    }
}