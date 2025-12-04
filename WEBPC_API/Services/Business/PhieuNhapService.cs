using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Models.Entities;
using WEBPC_API.Models.Interfaces;
using WEBPC_API.Repositories.Interfaces;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Services.Business
{
    public class PhieuNhapService : IPhieuNhapService
    {
        private readonly IPhieuNhapRepository _repo;
        private readonly ISanPhamRepository _sanPhamRepo; // Cần repo này để lấy thông tin SP nếu cần validate

        public PhieuNhapService(IPhieuNhapRepository repo, ISanPhamRepository sanPhamRepo)
        {
            _repo = repo;
            _sanPhamRepo = sanPhamRepo;
        }

        public async Task<List<PhieuNhapResponse>> GetAllPhieuNhapAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<PhieuNhapResponse> GetPhieuNhapByIdAsync(int id)
        {
            var pn = await _repo.GetByIdAsync(id);
            if (pn == null) throw new KeyNotFoundException("Không tìm thấy phiếu nhập");
            return MapToResponse(pn);
        }

        public async Task<PhieuNhapResponse> CreatePhieuNhapAsync(CreatePhieuNhapRequest request)
        {
            // 1. Tạo mã phiếu tự động (VD: PN + yyyyMMdd + Random)
            string maCode = $"PN{DateTime.Now:yyyyMMdd}{new Random().Next(1000, 9999)}";

            // 2. Map Entity
            var phieuNhap = new PhieuNhap
            {
                MaCodePhieu = maCode,
                MaNhanVienNhap = request.MaNhanVienNhap,
                GhiChu = request.GhiChu,
                NgayNhap = DateTime.Now,
                ChiTietPhieuNhaps = new List<ChiTietPhieuNhap>()
            };

            // 3. Xử lý chi tiết & Tính tổng tiền
            decimal tongTien = 0;
            foreach (var item in request.ChiTiet)
            {
                var ct = new ChiTietPhieuNhap
                {
                    MaSanPham = item.MaSanPham,
                    SoLuongNhap = item.SoLuongNhap,
                    GiaNhap = item.GiaNhap
                };
                tongTien += (item.SoLuongNhap * item.GiaNhap);
                phieuNhap.ChiTietPhieuNhaps.Add(ct);
            }
            phieuNhap.TongTienNhap = tongTien;

            // 4. Lưu vào DB (Trigger trong SQL sẽ tự động Update Tồn Kho SanPham)
            var newPhieu = await _repo.CreateAsync(phieuNhap);

            // 5. Trả về response (Reload để lấy thông tin include nếu cần)
            return await GetPhieuNhapByIdAsync(newPhieu.MaPhieuNhap);
        }

        public async Task<PhieuNhapResponse> UpdatePhieuNhapAsync(int id, UpdatePhieuNhapRequest request)
        {
            var phieuNhap = await _repo.GetByIdAsync(id);
            if (phieuNhap == null) throw new KeyNotFoundException("Không tìm thấy phiếu nhập");

            // Chỉ cho phép update Ghi chú. 
            // Nếu muốn update số lượng/giá thì phải xử lý logic phức tạp hơn với Trigger DB (Hoàn kho cũ -> Trừ kho mới)
            if (request.GhiChu != null) phieuNhap.GhiChu = request.GhiChu;

            await _repo.UpdateAsync(phieuNhap);
            return MapToResponse(phieuNhap);
        }

        public async Task<bool> DeletePhieuNhapAsync(int id)
        {
            // Khi xóa phiếu nhập, Trigger 'trg_CapNhatTonKho_NhapHang' (phần Deleted) 
            // sẽ tự động trừ lại số lượng tồn kho của sản phẩm.
            var phieuNhap = await _repo.GetByIdAsync(id);
            if (phieuNhap == null) return false;

            await _repo.DeleteAsync(id);
            return true;
        }

        // Helper Method
        private PhieuNhapResponse MapToResponse(PhieuNhap pn)
        {
            return new PhieuNhapResponse
            {
                MaPhieuNhap = pn.MaPhieuNhap,
                MaCodePhieu = pn.MaCodePhieu,
                NgayNhap = pn.NgayNhap,
                TongTienNhap = pn.TongTienNhap,
                TenNhanVien = pn.NhanVien != null ? pn.NhanVien.HoTen : "N/A",
                GhiChu = pn.GhiChu,
                ChiTiet = pn.ChiTietPhieuNhaps.Select(ct => new ChiTietPhieuNhapResponse
                {
                    MaChiTietPhieuNhap = ct.MaChiTietPhieuNhap,
                    MaSanPham = ct.MaSanPham,
                    TenSanPham = ct.SanPham != null ? ct.SanPham.TenSanPham : "Unknown",
                    SoLuongNhap = ct.SoLuongNhap,
                    GiaNhap = ct.GiaNhap
                }).ToList()
            };
        }
    }
}