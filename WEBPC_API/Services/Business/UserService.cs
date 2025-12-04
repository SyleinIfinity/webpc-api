using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEBPC_API.Models.Entities;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Repositories.Interfaces;
using WEBPC_API.Services.Interfaces;
using BC = BCrypt.Net.BCrypt; // Đặt biệt danh là "BC" để tránh nhầm lẫn

namespace WEBPC_API.Services.Business
{
    public class UserService : IUserService
    {
        private readonly INhanVienRepository _nvRepo;
        private readonly ITaiKhoanRepository _tkRepo;
        private readonly IVaiTroRepository _vtRepo;
        private readonly IKhachHangRepository _khRepo;

        public UserService(
            INhanVienRepository nvRepo,
            ITaiKhoanRepository tkRepo,
            IVaiTroRepository vtRepo,
            IKhachHangRepository khRepo)
        {
            _nvRepo = nvRepo;
            _tkRepo = tkRepo;
            _vtRepo = vtRepo;
            _khRepo = khRepo;
        }

        // ==========================================================
        // PHẦN 1: QUẢN LÝ VAI TRÒ
        // ==========================================================

        public async Task<List<VaiTroResponse>> GetAllVaiTros()
        {
            var list = await _vtRepo.GetAllAsync();
            return list.Select(x => new VaiTroResponse
            {
                MaVaiTro = x.MaVaiTro,
                TenVaiTro = x.TenVaiTro,
                MoTa = x.MoTa
            }).ToList();
        }

        public async Task<bool> CreateVaiTro(VaiTroRequest req)
        {
            var vt = new VaiTro
            {
                TenVaiTro = req.TenVaiTro,
                MoTa = req.MoTa
            };
            await _vtRepo.AddAsync(vt);
            return true;
        }

        public async Task<bool> DeleteVaiTro(int id)
        {
            await _vtRepo.DeleteAsync(id);
            return true;
        }

        // ==========================================================
        // PHẦN 2: QUẢN LÝ NHÂN VIÊN
        // ==========================================================

        public async Task<List<NhanVienResponse>> GetAllNhanViens()
        {
            var list = await _nvRepo.GetAllAsync();

            return list.Select(x => new NhanVienResponse
            {
                MaNhanVien = x.MaNhanVien,
                MaCodeNhanVien = x.MaCodeNhanVien,
                HoTen = x.HoTen,
                SoDienThoai = x.SoDienThoai,

                MaVaiTro = x.MaVaiTro,
                TenVaiTro = x.VaiTro != null ? x.VaiTro.TenVaiTro : "N/A",

                MaTaiKhoan = x.MaTaiKhoan,
                TenDangNhap = x.TaiKhoan != null ? x.TaiKhoan.TenDangNhap : "N/A",
                Email = x.TaiKhoan != null ? x.TaiKhoan.Email : "",
                TrangThaiTaiKhoan = x.TaiKhoan != null ? x.TaiKhoan.TrangThai : "Unknown"
            }).ToList();
        }

        public async Task<NhanVienResponse?> GetNhanVienById(int id)
        {
            var x = await _nvRepo.GetByIdAsync(id);
            if (x == null) return null;

            return new NhanVienResponse
            {
                MaNhanVien = x.MaNhanVien,
                MaCodeNhanVien = x.MaCodeNhanVien,
                HoTen = x.HoTen,
                SoDienThoai = x.SoDienThoai,

                MaVaiTro = x.MaVaiTro,
                TenVaiTro = x.VaiTro?.TenVaiTro ?? "N/A",

                MaTaiKhoan = x.MaTaiKhoan,
                TenDangNhap = x.TaiKhoan?.TenDangNhap ?? "N/A",
                Email = x.TaiKhoan?.Email ?? "",
                TrangThaiTaiKhoan = x.TaiKhoan?.TrangThai ?? "Unknown"
            };
        }

        public async Task<(bool IsSuccess, string Message)> CreateNhanVien(NhanVienRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.TenDangNhap) || string.IsNullOrWhiteSpace(req.MatKhau))
            {
                return (false, "Tên đăng nhập và mật khẩu không được để trống.");
            }

            if (await _tkRepo.IsUsernameExistsAsync(req.TenDangNhap))
            {
                return (false, $"Tên đăng nhập '{req.TenDangNhap}' đã tồn tại.");
            }
            if (await _tkRepo.IsEmailExistsAsync(req.Email))
            {
                return (false, $"Email '{req.Email}' đã được sử dụng.");
            }

            var newTaiKhoan = new TaiKhoan
            {
                TenDangNhap = req.TenDangNhap,
                Email = req.Email,
                // --- SỬA LỖI TẠI ĐÂY ---
                // Dùng trực tiếp BCrypt.HashPassword vì đã using BCrypt.Net ở trên
                MatKhauHash = BC.HashPassword(req.MatKhau),
                TrangThai = "Active",
                NgayTao = DateTime.Now
            };

            await _tkRepo.AddAsync(newTaiKhoan);

            var newNhanVien = new NhanVien
            {
                MaCodeNhanVien = "NV" + new Random().Next(10000, 99999),
                HoTen = req.HoTen,
                SoDienThoai = req.SoDienThoai,
                MaVaiTro = req.MaVaiTro,
                MaTaiKhoan = newTaiKhoan.MaTaiKhoan
            };

            await _nvRepo.AddAsync(newNhanVien);

            return (true, "Tạo nhân viên thành công.");
        }

        public async Task<bool> UpdateNhanVien(int id, NhanVienRequest req)
        {
            var nv = await _nvRepo.GetByIdAsync(id);
            if (nv == null) return false;

            nv.HoTen = req.HoTen;
            nv.SoDienThoai = req.SoDienThoai;
            nv.MaVaiTro = req.MaVaiTro;

            await _nvRepo.UpdateAsync(nv);
            return true;
        }

        public async Task<bool> DeleteNhanVien(int id)
        {
            var nv = await _nvRepo.GetByIdAsync(id);
            if (nv == null) return false;

            await _nvRepo.DeleteAsync(id);
            if (nv.MaTaiKhoan > 0)
            {
                await _tkRepo.DeleteAsync(nv.MaTaiKhoan);
            }

            return true;
        }

        // ==========================================================
        // PHẦN 3: QUẢN LÝ KHÁCH HÀNG
        // ==========================================================

        public async Task<List<KhachHangResponse>> GetAllKhachHangs()
        {
            var list = await _khRepo.GetAllAsync();

            return list.Select(x => new KhachHangResponse
            {
                MaKhachHang = x.MaKhachHang,
                HoTen = x.HoTen,
                SoDienThoai = x.SoDienThoai,
                Email = x.Email ?? "",
                MaTaiKhoan = x.MaTaiKhoan,
                TenDangNhap = x.TaiKhoan?.TenDangNhap,
                CoTaiKhoan = x.MaTaiKhoan != null && x.MaTaiKhoan > 0
            }).ToList();
        }

        public async Task<(bool IsSuccess, string Message)> CreateKhachHang(KhachHangRequest req)
        {
            int? taiKhoanId = null;

            if (!string.IsNullOrEmpty(req.TenDangNhap) && !string.IsNullOrEmpty(req.MatKhau))
            {
                if (await _tkRepo.IsUsernameExistsAsync(req.TenDangNhap))
                {
                    return (false, "Tên đăng nhập đã tồn tại.");
                }

                if (!string.IsNullOrEmpty(req.Email))
                {
                    if (await _tkRepo.IsEmailExistsAsync(req.Email))
                    {
                        return (false, "Email đã được sử dụng cho tài khoản khác.");
                    }
                }

                var tk = new TaiKhoan
                {
                    TenDangNhap = req.TenDangNhap,
                    Email = req.Email ?? "",
                    // --- SỬA LỖI TẠI ĐÂY ---
                    MatKhauHash = BC.HashPassword(req.MatKhau),
                    TrangThai = "Active",
                    NgayTao = DateTime.Now
                };

                await _tkRepo.AddAsync(tk);
                taiKhoanId = tk.MaTaiKhoan;
            }

            var kh = new KhachHang
            {
                HoTen = req.HoTen,
                SoDienThoai = req.SoDienThoai,
                Email = req.Email,
                MaTaiKhoan = taiKhoanId
            };

            await _khRepo.AddAsync(kh);
            return (true, "Thêm khách hàng thành công.");
        }

        public async Task<bool> UpdateKhachHang(int id, UpdateKhachHangRequest req)
        {
            // 1. Tìm khách hàng theo ID
            var kh = await _khRepo.GetByIdAsync(id);
            if (kh == null) return false;

            // 2. Cập nhật thông tin
            kh.HoTen = req.HoTen;
            kh.SoDienThoai = req.SoDienThoai;
            kh.Email = req.Email;

            // Lưu ý: Chỉ update thông tin liên hệ trong bảng KhachHang.
            // Nếu muốn đổi Email đăng nhập (trong bảng TaiKhoan), cần dùng API riêng của TaiKhoan để đảm bảo bảo mật.

            // 3. Lưu xuống DB
            await _khRepo.UpdateAsync(kh);
            return true;
        }

        // ==========================================================
        // PHẦN 4: QUẢN LÝ TÀI KHOẢN
        // ==========================================================

        public async Task<List<TaiKhoanResponse>> GetAllTaiKhoans()
        {
            var list = await _tkRepo.GetAllAsync();

            return list.Select(x => new TaiKhoanResponse
            {
                MaTaiKhoan = x.MaTaiKhoan,
                TenDangNhap = x.TenDangNhap,
                Email = x.Email,
                TrangThai = x.TrangThai,
                NgayTao = x.NgayTao
            }).ToList();
        }

        public async Task<bool> ChangePassword(int id, string newPass)
        {
            var tk = await _tkRepo.GetByIdAsync(id);
            if (tk == null) return false;

            // --- SỬA LỖI TẠI ĐÂY ---
            tk.MatKhauHash = BC.HashPassword(newPass);

            await _tkRepo.UpdateAsync(tk);
            return true;
        }
    }
}