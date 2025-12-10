using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WEBPC_API.Data;
using WEBPC_API.Helpers;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Models.Entities; // Đảm bảo namespace này đúng với dự án của bạn
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
        private readonly DataContext _context;
        private readonly IConfiguration _configuration; // Biến mới để đọc file appsettings
        private readonly FileUploadHelper _fileHelper;

        // Cập nhật Constructor để nhận thêm IConfiguration
        public UserService(
            INhanVienRepository nvRepo,
            ITaiKhoanRepository tkRepo,
            IVaiTroRepository vtRepo,
            IKhachHangRepository khRepo,
            DataContext context,               // <--- Thêm cái này
            IConfiguration configuration,      // <--- Thêm cái này
            FileUploadHelper fileHelper)
        {
            _nvRepo = nvRepo;
            _tkRepo = tkRepo;
            _vtRepo = vtRepo;
            _khRepo = khRepo;
            _context = context;               // <--- Gán giá trị để không bị Null
            _configuration = configuration;   // <--- Gán giá trị để không bị Null
            _fileHelper = fileHelper;
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

        public async Task<LoginResponse?> Login(LoginRequest request)
        {
            // 1. Tìm tài khoản VÀ Include cả NhanVien lẫn KhachHang
            var user = await _context.TaiKhoans
                .Include(t => t.NhanVien)
                    .ThenInclude(nv => nv.VaiTro)
                .Include(t => t.KhachHang) // <--- QUAN TRỌNG: Phải Include thêm dòng này
                .FirstOrDefaultAsync(x => x.TenDangNhap == request.TenDangNhap);

            if (user == null) return null;

            // 2. Kiểm tra mật khẩu (Giữ nguyên logic cũ của bạn)
            bool isPasswordValid = false;
            if (!string.IsNullOrEmpty(user.MatKhauHash))
            {
                if (user.MatKhauHash.StartsWith("$2"))
                {
                    try { isPasswordValid = BC.Verify(request.MatKhau, user.MatKhauHash); } catch { }
                }
                if (!isPasswordValid && user.MatKhauHash.Trim() == request.MatKhau.Trim())
                {
                    isPasswordValid = true;
                }
            }
            if (!isPasswordValid) return null;

            // 3. Xử lý thông tin hiển thị (PHẦN SỬA ĐỔI)
            int? maNhanVien = null;
            int? maKhachHang = null;
            string hoTen = "Unknown";
            string tenVaiTro = "Unknown";
            int maVaiTro = 0;

            // Kiểm tra xem Tài khoản này liên kết với Nhân Viên hay Khách Hàng
            if (user.NhanVien != null)
            {
                // === LÀ NHÂN VIÊN ===
                maNhanVien = user.NhanVien.MaNhanVien;
                hoTen = user.NhanVien.HoTen;
                maVaiTro = user.NhanVien.VaiTro?.MaVaiTro ?? 0;
                tenVaiTro = user.NhanVien.VaiTro?.TenVaiTro ?? "NhanVien";
            }
            else if (user.KhachHang != null)
            {
                // === LÀ KHÁCH HÀNG ===
                maKhachHang = user.KhachHang.MaKhachHang;
                hoTen = user.KhachHang.HoTen;
                // Khách hàng thường không có bảng VaiTro riêng, ta gán cứng hoặc quy định ID
                maVaiTro = -1;
                tenVaiTro = "KhachHang";
            }

            // 4. Tạo Token (Cần cập nhật hàm này ở Bước 3)
            string token = GenerateJwtToken(user, maNhanVien, maKhachHang, tenVaiTro);

            return new LoginResponse
            {
                MaNhanVien = maNhanVien,
                MaKhachHang = maKhachHang, // Trả về mã khách hàng
                HoTen = hoTen,
                TenDangNhap = user.TenDangNhap,
                MaVaiTro = maVaiTro,
                TenVaiTro = tenVaiTro,
                Token = token
            };
        }

        // --- [MỚI] HÀM CẬP NHẬT ẢNH ĐẠI DIỆN ---
        public async Task<string> UpdateAvatarAsync(int id, IFormFile file)
        {
            // 1. Tìm tài khoản
            var user = await _tkRepo.GetByIdAsync(id);
            if (user == null) throw new Exception("Tài khoản không tồn tại.");

            // 2. Xóa ảnh cũ trên Cloudinary (Nếu có)
            // Lưu ý: Sửa 'HinhAnh' thành 'AnhDaiDien'
            if (!string.IsNullOrEmpty(user.AnhDaiDien))
            {
                try
                {
                    string publicId = GetPublicIdFromUrl(user.AnhDaiDien);
                    if (!string.IsNullOrEmpty(publicId))
                    {
                        // Gọi hàm xóa từ Helper
                        await _fileHelper.DeleteImageAsync(publicId);
                    }
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi nhưng không chặn quy trình (để vẫn cho up ảnh mới)
                    Console.WriteLine($"[CẢNH BÁO] Lỗi xóa ảnh cũ: {ex.Message}");
                }
            }

            // 3. Upload ảnh mới lên Cloudinary
            // Gọi đúng hàm dành cho Avatar
            var uploadResult = await _fileHelper.UploadAvatarAsync(file);
            if (string.IsNullOrEmpty(uploadResult))
                throw new Exception("Lỗi khi upload ảnh mới lên Cloud.");

            // 4. Cập nhật đường dẫn mới vào DB
            user.AnhDaiDien = uploadResult; // <--- Đã sửa đúng tên biến
            await _tkRepo.UpdateAsync(user);

            return user.AnhDaiDien;
        }

        // --- [MỚI] HÀM CẬP NHẬT TRẠNG THÁI ---
        public async Task<bool> UpdateStatusAsync(int id, string trangThaiMoi)
        {
            var user = await _tkRepo.GetByIdAsync(id);
            if (user == null) throw new Exception("Tài khoản không tồn tại.");

            // Kiểm tra giá trị hợp lệ (Active/Locked)
            if (trangThaiMoi != "Active" && trangThaiMoi != "Locked")
                throw new Exception("Trạng thái không hợp lệ (Chỉ chấp nhận 'Active' hoặc 'Locked').");

            user.TrangThai = trangThaiMoi;
            await _tkRepo.UpdateAsync(user);
            return true;
        }

        // Cập nhật tham số đầu vào
        private string GenerateJwtToken(TaiKhoan user, int? maNhanVien, int? maKhachHang, string roleName)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            var claims = new List<Claim>
    {
        new Claim("MaTaiKhoan", user.MaTaiKhoan.ToString()),
        new Claim(ClaimTypes.Name, user.TenDangNhap),
        new Claim(ClaimTypes.Role, roleName)
    };

            // Nếu là nhân viên thì thêm Claim MaNhanVien
            if (maNhanVien.HasValue)
            {
                claims.Add(new Claim("MaNhanVien", maNhanVien.Value.ToString()));
            }

            // Nếu là khách hàng thì thêm Claim MaKhachHang
            if (maKhachHang.HasValue)
            {
                claims.Add(new Claim("MaKhachHang", maKhachHang.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        // Hàm phụ trợ tách PublicID từ URL Cloudinary
        private string GetPublicIdFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                string path = uri.AbsolutePath;
                string tag = "upload/";
                int idx = path.IndexOf(tag);
                if (idx != -1)
                {
                    string part = path.Substring(idx + tag.Length);
                    int slashIdx = part.IndexOf('/');
                    string publicIdWithExt = part.Substring(slashIdx + 1);
                    return publicIdWithExt.Substring(0, publicIdWithExt.LastIndexOf('.'));
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}