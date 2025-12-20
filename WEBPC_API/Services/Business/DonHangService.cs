using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Models.Entities;
using WEBPC_API.Models.Enums;
using WEBPC_API.Repositories.Interfaces;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Services.Business
{
    public class DonHangService : IDonHangService
    {
        private readonly IDonHangRepository _donHangRepo;
        private readonly IGioHangRepository _gioHangRepo;
        private readonly DataContext _context;

        public DonHangService(IDonHangRepository donHangRepo, IGioHangRepository gioHangRepo, DataContext context)
        {
            _donHangRepo = donHangRepo;
            _gioHangRepo = gioHangRepo;
            _context = context;
        }

        // --- 1. LOGIC TẠO ĐƠN HÀNG (FULL: SHIP + VOUCHER) ---
        public async Task<DonHang> CreateOrderAsync(TaoDonHangRequest request)
        {
            // 1. Validate cơ bản
            if (string.IsNullOrEmpty(request.PhuongThucThanhToan) ||
               (request.PhuongThucThanhToan != "COD" && request.PhuongThucThanhToan != "VietQR"))
            {
                throw new Exception("Phương thức thanh toán không hợp lệ (COD hoặc VietQR).");
            }

            if (request.SelectedCartItemIds == null || !request.SelectedCartItemIds.Any())
                throw new Exception("Vui lòng chọn ít nhất một sản phẩm để thanh toán.");

            // 2. Lấy giỏ hàng
            var gioHang = await _gioHangRepo.GetByKhachHangIdAsync(request.MaKhachHang);
            if (gioHang == null || gioHang.ChiTietGioHangs == null || !gioHang.ChiTietGioHangs.Any())
                throw new Exception("Giỏ hàng trống hoặc không tồn tại.");

            // 3. Lọc sản phẩm user chọn mua
            var selectedItems = gioHang.ChiTietGioHangs
                .Where(item => request.SelectedCartItemIds.Contains(item.MaChiTietGioHang))
                .ToList();

            if (!selectedItems.Any())
                throw new Exception("Sản phẩm chọn mua không hợp lệ.");

            // 4. Bắt đầu Transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // -- A. Tính Tổng tiền hàng (Subtotal) --
                decimal tongTienHang = 0;
                var listChiTietDonHang = new List<ChiTietDonHang>();

                foreach (var itemCart in selectedItems)
                {
                    if (itemCart.SanPham == null) continue;

                    // Lấy giá ưu đãi nếu có
                    decimal donGiaThucTe = (itemCart.SanPham.GiaKhuyenMai.HasValue && itemCart.SanPham.GiaKhuyenMai.Value > 0)
                                           ? itemCart.SanPham.GiaKhuyenMai.Value
                                           : itemCart.SanPham.GiaBan;

                    var chiTiet = new ChiTietDonHang
                    {
                        maSanPham = itemCart.MaSanPham,
                        soLuong = itemCart.SoLuong,
                        donGiaLucMua = donGiaThucTe,
                        thanhTien = itemCart.SoLuong * donGiaThucTe
                    };

                    listChiTietDonHang.Add(chiTiet);
                    tongTienHang += chiTiet.thanhTien;
                }

                // -- B. Tính Giảm giá (Voucher) --
                decimal tienGiam = 0;
                if (!string.IsNullOrEmpty(request.MaCodeVoucher))
                {
                    // Tìm khuyến mãi theo mã Code & Còn hạn
                    // Lưu ý: Kiểm tra kỹ tên thuộc tính trong Entity KhuyenMai của em (PascalCase hay camelCase)
                    // Ở đây thầy dùng PascalCase (MaCodeKM) làm chuẩn. Nếu DB em dùng maCodeKM thì sửa lại nhé.
                    var voucher = await _context.KhuyenMais
                        .FirstOrDefaultAsync(k => k.MaCodeKM == request.MaCodeVoucher &&
                                                  k.NgayBatDau <= DateTime.Now &&
                                                  k.NgayKetThuc > DateTime.Now);

                    if (voucher != null)
                    {
                        // Kiểm tra đơn tối thiểu
                        if (voucher.DonHangToiThieu <= tongTienHang)
                        {
                            if (voucher.LoaiGiam == "DIRECT")
                            {
                                tienGiam = voucher.GiaTriGiam;
                            }
                            else // PERCENT
                            {
                                tienGiam = (tongTienHang * voucher.GiaTriGiam) / 100;
                                // Kiểm tra trần giảm tối đa (nếu có)
                                if (voucher.GiamToiDa.HasValue && voucher.GiamToiDa.Value > 0 && tienGiam > voucher.GiamToiDa.Value)
                                {
                                    tienGiam = voucher.GiamToiDa.Value;
                                }
                            }
                        }
                    }
                }

                // -- C. Phí vận chuyển --
                decimal phiShip = request.PhiVanChuyen; // Lấy từ Client gửi lên

                // -- D. Tổng thanh toán cuối cùng --
                decimal tongThanhToan = tongTienHang + phiShip - tienGiam;
                if (tongThanhToan < 0) tongThanhToan = 0;

                // -- E. Tạo Đơn Hàng --
                string maCode = "DH" + DateTime.Now.ToString("yyMMdd") + new Random().Next(1000, 9999);

                var donHang = new DonHang
                {
                    maCodeDonHang = maCode,
                    maKhachHang = request.MaKhachHang,
                    ngayDat = DateTime.Now,
                    trangThai = request.PhuongThucThanhToan == "VietQR" ? TrangThaiDonHang.ChoThanhToan.ToString() : TrangThaiDonHang.ChoXacNhan.ToString(),
                    phuongThucThanhToan = request.PhuongThucThanhToan,
                    nguoiNhan = request.NguoiNhan,
                    soDienThoaiGiao = request.SoDienThoai,
                    diaChiGiaoHang = request.DiaChiGiaoHang,

                    // Lưu các giá trị tiền
                    phiVanChuyen = phiShip,
                    tongTien = tongThanhToan, // Đã trừ giảm giá và cộng ship

                    // Gán chi tiết
                    ChiTietDonHangs = listChiTietDonHang
                };

                // Lưu vào DB
                await _donHangRepo.AddAsync(donHang);

                // Xóa giỏ hàng
                await _gioHangRepo.RemoveCartItemsAsync(gioHang.MaGioHang, request.SelectedCartItemIds);

                // Commit
                await transaction.CommitAsync();

                return donHang;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Đặt hàng thất bại: " + ex.Message);
            }
        }

        // --- 2. GET ALL ORDERS ---
        public async Task<List<DonHangResponse>> GetAllOrdersAsync()
        {
            var orders = await _donHangRepo.GetAllOrdersFullAsync();
            // Map Entity -> DTO Response
            return orders.Select(MapToResponse).ToList();
        }

        // --- 3. GET ORDER BY ID ---
        public async Task<DonHangResponse?> GetOrderByIdAsync(int id)
        {
            var order = await _donHangRepo.GetOrderByIdFullAsync(id);
            if (order == null) return null;
            return MapToResponse(order);
        }

        // --- 4. GET ORDERS BY CUSTOMER ---
        public async Task<List<DonHangResponse>> GetOrdersByCustomerAsync(int maKhachHang)
        {
            var orders = await _donHangRepo.GetOrdersByCustomerIdAsync(maKhachHang);
            return orders.Select(MapToResponse).ToList();
        }

        // --- HELPER MAPPING ---
        private DonHangResponse MapToResponse(DonHang donHang)
        {
            return new DonHangResponse
            {
                MaDonHang = donHang.maDonHang,
                MaCodeDonHang = donHang.maCodeDonHang,
                MaKhachHang = donHang.maKhachHang,
                NgayDat = donHang.ngayDat,
                TrangThai = donHang.trangThai,

                // [MỚI] Map phương thức thanh toán sang Response
                PhuongThucThanhToan = donHang.phuongThucThanhToan,

                NguoiNhan = donHang.nguoiNhan,
                SoDienThoaiGiao = donHang.soDienThoaiGiao,
                DiaChiGiaoHang = donHang.diaChiGiaoHang,
                PhiVanChuyen = donHang.phiVanChuyen,
                TongTien = donHang.tongTien,
                ChiTiet = donHang.ChiTietDonHangs?.Select(ct => new ChiTietDonHangResponse
                {
                    MaSanPham = ct.maSanPham,
                    TenSanPham = ct.SanPham?.TenSanPham ?? "Sản phẩm đã xóa",
                    HinhAnh = ct.SanPham?.HinhAnhs?.FirstOrDefault(h => h.LaAnhDaiDien)?.UrlHinhAnh
                              ?? ct.SanPham?.HinhAnhs?.FirstOrDefault()?.UrlHinhAnh
                              ?? "/images/no-image.png",
                    SoLuong = ct.soLuong,
                    DonGiaLucMua = ct.donGiaLucMua,
                    ThanhTien = ct.thanhTien
                }).ToList() ?? new List<ChiTietDonHangResponse>(),

                GiaoDichs = donHang.GiaoDichThanhToans?.Select(gd => new GiaoDichResponse
                {
                    MaGiaoDich = gd.maGiaoDich,
                    NgayGiaoDich = gd.ngayTao,
                    SoTien = gd.soTien,
                    TrangThai = gd.trangThai,
                    PhuongThuc = gd.phuongThuc
                }).ToList() ?? new List<GiaoDichResponse>() // Handle null
            };
        }

        public async Task CancelOrderAsync(int orderId, CancelOrderRequest request)
        {
            // 1. Lấy đơn hàng kèm chi tiết (để trả kho)
            var donHang = await _context.DonHang
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.maDonHang == orderId);

            if (donHang == null) throw new Exception("Đơn hàng không tồn tại.");

            // 2. [BẢO MẬT] Kiểm tra chủ sở hữu
            if (donHang.maKhachHang != request.MaKhachHang)
            {
                throw new Exception("Bạn không có quyền hủy đơn hàng này.");
            }

            // 3. Kiểm tra trạng thái có được phép hủy không?
            // Chỉ cho hủy khi: Chờ xác nhận (COD) HOẶC Chờ thanh toán (QR chưa trả tiền)
            if (donHang.trangThai != TrangThaiDonHang.ChoXacNhan.ToString() &&
                donHang.trangThai != TrangThaiDonHang.ChoThanhToan.ToString())
            {
                // Nếu đã thanh toán hoặc đang giao -> Không cho hủy
                throw new Exception($"Đơn hàng đang ở trạng thái '{donHang.trangThai}', không thể hủy. Vui lòng liên hệ CSKH.");
            }

            // 4. Thực hiện Hủy
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // A. Đổi trạng thái đơn
                    donHang.trangThai = TrangThaiDonHang.Huy.ToString();

                    // (Tùy chọn) Có thể lưu Lý do hủy vào một bảng Log hoặc Note (nếu có)
                    // Ở đây mình tạm thời chưa lưu lý do vào DB vì bảng DonHang chưa có cột Note

                    // B. Xử lý Giao dịch thanh toán (Nếu đang Pending -> Failed)
                    var pendingTrans = await _context.GiaoDichThanhToan
                        .FirstOrDefaultAsync(g => g.maDonHang == orderId &&
                                                  g.trangThai == TrangThaiThanhToan.Pending.ToString());

                    if (pendingTrans != null)
                    {
                        pendingTrans.trangThai = TrangThaiThanhToan.Failed.ToString();
                        pendingTrans.noiDungLoi = $"User Cancel: {request.LyDoHuy}";
                    }

                    // C. [QUAN TRỌNG] Hoàn lại số lượng Tồn Kho
                    if (donHang.ChiTietDonHangs != null)
                    {
                        foreach (var item in donHang.ChiTietDonHangs)
                        {
                            // Lưu ý: Dùng _context.SanPhams (hoặc SanPham tùy DBContext của bạn)
                            // Dựa trên các file trước, context của bạn có thể là SanPham (số ít) hoặc SanPhams (số nhiều)
                            // Hãy check lại DataContext.cs. Theo file bạn gửi là 'SanPhams' (số nhiều)
                            var product = await _context.SanPhams.FindAsync(item.maSanPham);

                            if (product != null)
                            {
                                product.SoLuongTon += item.soLuong;
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        // =========================================================
        // 1. DUYỆT ĐƠN HÀNG (Approve)
        // =========================================================
        public async Task<bool> ApproveOrderAsync(int maDonHang)
        {
            // Lấy đơn hàng kèm chi tiết và giao dịch
            var donHang = await _donHangRepo.GetOrderByIdFullAsync(maDonHang);

            if (donHang == null)
                throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{maDonHang}");

            // --- LOGIC KIỂM TRA THEO ENTITY CAMELCASE ---
            if (donHang.phuongThucThanhToan == "COD")
            {
                // COD: Kiểm tra trạng thái (viết thường 'trangThai')
                if (donHang.trangThai != "ChoXacNhan")
                {
                    throw new InvalidOperationException("Đơn hàng COD chỉ được duyệt khi đang ở trạng thái 'Chờ xác nhận'.");
                }
            }
            else if (donHang.phuongThucThanhToan == "VietQR")
            {
                // VietQR: Kiểm tra bảng giao dịch (viết thường 'trangThai', 'ngayTao')

                var transaction = donHang.GiaoDichThanhToans?
                    .OrderByDescending(x => x.ngayTao) // Dùng 'ngayTao' thay vì NgayGiaoDich
                    .FirstOrDefault(x => x.trangThai == "Success"); // Dùng 'trangThai'

                if (transaction == null)
                {
                    throw new InvalidOperationException("Đơn hàng VietQR chưa được thanh toán thành công. Không thể duyệt.");
                }

                // Kiểm tra trạng thái đơn
                if (donHang.trangThai != "ChoThanhToan" && donHang.trangThai != "ChoXacNhan" && donHang.trangThai != "DaThanhToan")
                {
                    throw new InvalidOperationException($"Trạng thái đơn hàng không hợp lệ để duyệt ({donHang.trangThai}).");
                }
            }
            else
            {
                throw new InvalidOperationException("Phương thức thanh toán không hỗ trợ duyệt tự động.");
            }

            // --- THỰC HIỆN DUYỆT ---
            donHang.trangThai = "DangGiao"; // Update 'trangThai'

            await _donHangRepo.UpdateAsync(donHang);
            return true;
        }

        // =========================================================
        // 2. TỪ CHỐI ĐƠN HÀNG (Reject)
        // =========================================================
        public async Task<bool> RejectOrderAsync(int maDonHang, RejectOrderRequest request)
        {
            var donHang = await _donHangRepo.GetOrderByIdFullAsync(maDonHang);

            if (donHang == null)
                throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{maDonHang}");

            // --- LOGIC KIỂM TRA ---
            if (donHang.phuongThucThanhToan == "COD")
            {
                if (donHang.trangThai != "ChoXacNhan")
                {
                    throw new InvalidOperationException("Chỉ có thể từ chối đơn COD khi đang 'Chờ xác nhận'.");
                }
            }
            else if (donHang.phuongThucThanhToan == "VietQR")
            {
                // Check đã trả tiền chưa (dùng 'trangThai')
                var daThanhToan = donHang.GiaoDichThanhToans != null &&
                                  donHang.GiaoDichThanhToans.Any(x => x.trangThai == "Success");

                if (daThanhToan)
                {
                    throw new InvalidOperationException("Đơn hàng VietQR đã thanh toán thành công. Vui lòng thực hiện quy trình Hoàn tiền/Hủy đặc biệt.");
                }
            }

            // --- TỪ CHỐI (HỦY) ---
            donHang.trangThai = "Huy";

            // --- HOÀN LẠI KHO ---
            if (donHang.ChiTietDonHangs != null)
            {
                foreach (var item in donHang.ChiTietDonHangs)
                {
                    // Dùng 'maSanPham' viết thường
                    var sanPham = await _context.SanPhams.FindAsync(item.maSanPham);
                    if (sanPham != null)
                    {
                        // Dùng 'soLuong' viết thường
                        sanPham.SoLuongTon += item.soLuong;
                    }
                }
            }

            await _donHangRepo.UpdateAsync(donHang);
            return true;
        }

        public async Task<bool> XacNhanNhanHangAsync(int maDonHang, int maKhachHang)
        {
            // 1. [SỬA TÊN DBSET]: Trong DataContext em đặt là DonHang (số ít)
            // 2. [SỬA TÊN BIẾN]: maDonHang (chữ thường)
            var donHang = await _context.DonHang.FirstOrDefaultAsync(x => x.maDonHang == maDonHang);

            // 3. [SỬA TÊN BIẾN]: maKhachHang (chữ thường)
            if (donHang == null || donHang.maKhachHang != maKhachHang) return false;

            // 4. [SỬA ENUM]: Trong file Enum tên là 'DangGiao', không phải 'DangVanChuyen'
            // Do DB lưu chuỗi, nên ta so sánh chuỗi
            if (donHang.trangThai != TrangThaiDonHang.DangGiao.ToString()) return false;

            // 5. Cập nhật trạng thái -> HoanThanh
            donHang.trangThai = TrangThaiDonHang.HoanThanh.ToString();

            // [LƯU Ý]: Entity DonHang của em KHÔNG CÓ cột 'trangThaiThanhToan', 
            // nên thầy bỏ đoạn update thanh toán đi để tránh lỗi.
            // Trạng thái 'HoanThanh' của đơn hàng đã ngầm hiểu là quy trình kết thúc (đã trả tiền).

            // 6. [SỬA LOG]:
            // - DbSet là NhatKyHoatDong (số ít)
            // - Entity không có MaNguoiDung, chỉ có MaNhanVien.
            // - Giải pháp: Ghi ID khách vào cột MoTa.
            _context.NhatKyHoatDong.Add(new NhatKyHoatDong
            {
                MaNhanVien = null, // Khách hàng làm, không phải nhân viên
                HanhDong = "Xác nhận nhận hàng", // [SỬA]: Mapping vào cột HanhDong
                MoTa = $"Khách hàng {maKhachHang} xác nhận hoàn thành đơn #{maDonHang}", // Ghi chi tiết vào đây
                ThoiGian = DateTime.Now
                // IPThucHien có thể để null hoặc lấy từ request nếu cần
            });

            await _context.SaveChangesAsync();
            return true;
        }
    }
}