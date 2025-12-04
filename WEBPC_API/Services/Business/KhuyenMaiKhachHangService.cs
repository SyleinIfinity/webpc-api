using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Services.Business
{
    public class KhuyenMaiKhachHangService : IKhuyenMaiKhachHangService
    {
        private readonly IKhuyenMaiKhachHangRepository _kmkhRepo;
        private readonly IKhuyenMaiRepository _kmRepo;

        public KhuyenMaiKhachHangService(IKhuyenMaiKhachHangRepository kmkhRepo, IKhuyenMaiRepository kmRepo)
        {
            _kmkhRepo = kmkhRepo;
            _kmRepo = kmRepo;
        }

        public async Task<IEnumerable<KhuyenMaiKhachHangResponse>> GetByKhachHangIdAsync(int maKhachHang)
        {
            var list = await _kmkhRepo.GetByKhachHangIdAsync(maKhachHang);
            return list.Select(x => new KhuyenMaiKhachHangResponse
            {
                MaKMKH = x.MaKMKH,
                MaKhuyenMai = x.MaKhuyenMai,
                MaKhachHang = x.MaKhachHang,
                DaSuDung = x.DaSuDung,
                NgayThuThap = x.NgayThuThap,
                // Lấy thông tin từ bảng cha include
                MaCodeKM = x.KhuyenMai?.MaCodeKM,
                TenChuongTrinh = x.KhuyenMai?.TenChuongTrinh
            });
        }

        public async Task<string> ThuThapKhuyenMaiAsync(ThuThapKhuyenMaiRequest request)
        {
            // 1. Kiểm tra Khuyến mãi có tồn tại và còn hạn không
            var km = await _kmRepo.GetByIdAsync(request.MaKhuyenMai);
            if (km == null) return "Mã khuyến mãi không tồn tại.";

            if (DateTime.Now < km.NgayBatDau || DateTime.Now > km.NgayKetThuc)
                return "Chương trình khuyến mãi chưa bắt đầu hoặc đã kết thúc.";

            if (km.SoLuongConLai <= 0)
                return "Mã khuyến mãi đã hết lượt sử dụng.";

            // 2. Kiểm tra khách hàng đã có mã này chưa
            var daCo = await _kmkhRepo.ExistsAsync(request.MaKhuyenMai, request.MaKhachHang);
            if (daCo) return "Bạn đã thu thập mã này rồi.";

            // 3. Thực hiện thu thập
            var kmkh = new KhuyenMaiKhachHang
            {
                MaKhuyenMai = request.MaKhuyenMai,
                MaKhachHang = request.MaKhachHang,
                DaSuDung = false,
                NgayThuThap = DateTime.Now
            };

            await _kmkhRepo.AddAsync(kmkh);

            // 4. Trừ số lượng tồn kho của mã khuyến mãi
            km.SoLuongConLai -= 1;
            await _kmRepo.UpdateAsync(km);

            return "Thu thập thành công!";
        }

        public async Task<bool> XoaKhuyenMaiCuaKhachAsync(int id)
        {
            var exists = await _kmkhRepo.GetByIdAsync(id);
            if (exists == null) return false;

            await _kmkhRepo.DeleteAsync(id);
            // Có hoàn lại số lượng cho mã cha hay không tùy nghiệp vụ, ở đây tôi giả sử xóa là mất luôn.
            return true;
        }
    }
}