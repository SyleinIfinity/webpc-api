using Microsoft.EntityFrameworkCore;
using WEBPC_API.Models.Entities;

namespace WEBPC_API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        // --- Nhóm Sản Phẩm & Danh Mục ---
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<HinhAnhSanPham> HinhAnhSanPhams { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<OtpLog> OtpLogs { get; set; }
        public DbSet<VaiTro> VaiTros { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<ThongSoKyThuat> ThongSoKyThuats { get; set; }
        public DbSet<KhuyenMai> KhuyenMais { get; set; }
        public DbSet<KhuyenMaiKhachHang> KhuyenMaiKhachHangs { get; set; }
        public DbSet<SoDiaChi> SoDiaChis { get; set; }
        public DbSet<PhieuNhap> PhieuNhaps { get; set; }
        public DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Cấu hình cho SanPham ---
            modelBuilder.Entity<SanPham>(entity =>
            {
                entity.HasKey(e => e.MaSanPham);
                entity.Property(e => e.TenSanPham).IsRequired().HasMaxLength(255);
                entity.Property(e => e.GiaBan).HasColumnType("decimal(18,2)");

                // Cấu hình mối quan hệ 1-N với HinhAnhSanPham
                entity.HasMany(e => e.HinhAnhs)
                      .WithOne(h => h.SanPham)
                      .HasForeignKey(h => h.MaSanPham)
                      .OnDelete(DeleteBehavior.Cascade);

                // --- SỬA LẠI ĐOẠN NÀY ---
                // Thay vì .HasOne<DanhMuc>(), hãy dùng lambda e => e.DanhMuc
                entity.HasOne(e => e.DanhMuc)
                      .WithMany(d => d.SanPhams)
                      .HasForeignKey(e => e.MaDanhMuc)
                      .OnDelete(DeleteBehavior.Restrict);

                // Khai báo Trigger
                entity.ToTable(tb => tb.HasTrigger("trg_XoaSanPham"));
            });

            // --- Cấu hình các bảng khác giữ nguyên ---
            modelBuilder.Entity<DanhMuc>(entity => { entity.HasKey(e => e.MaDanhMuc); });
            modelBuilder.Entity<HinhAnhSanPham>(entity => { entity.HasKey(e => e.Id); });
            modelBuilder.Entity<OtpLog>(entity =>
            {
                entity.HasKey(e => e.MaLog);
                entity.Property(e => e.TrangThai).HasDefaultValue("ConHan");
                entity.Property(e => e.ThoiGianTao).HasDefaultValueSql("GETDATE()");

                // --- BỔ SUNG DÒNG NÀY ---
                // Báo cho EF Core biết bảng này có Trigger để nó tránh dùng OUTPUT gây lỗi
                entity.ToTable(tb => tb.HasTrigger("trg_TuDongHetHanOTPCu"));
            });
            // TaiKhoan
            modelBuilder.Entity<TaiKhoan>(e => {
                e.HasIndex(x => x.TenDangNhap).IsUnique();
                e.HasIndex(x => x.Email).IsUnique();
            });

            // NhanVien (1-1 với TaiKhoan, N-1 với VaiTro)
            modelBuilder.Entity<NhanVien>(e => {
                e.HasOne(nv => nv.TaiKhoan)
                 .WithOne(tk => tk.NhanVien)
                 .HasForeignKey<NhanVien>(nv => nv.MaTaiKhoan);

                e.HasOne(nv => nv.VaiTro)
                 .WithMany(vt => vt.NhanViens)
                 .HasForeignKey(nv => nv.MaVaiTro);
            });

            // KhachHang (1-1 với TaiKhoan - optional)
            modelBuilder.Entity<KhachHang>(e => {
                e.HasOne(kh => kh.TaiKhoan)
                 .WithOne(tk => tk.KhachHang)
                 .HasForeignKey<KhachHang>(kh => kh.MaTaiKhoan);
            });

            // --- Cấu hình ThongSoKyThuat ---
            modelBuilder.Entity<ThongSoKyThuat>(entity =>
            {
                entity.HasKey(e => e.MaThongSo);
                entity.Property(e => e.TenThongSo).IsRequired().HasMaxLength(100);
                entity.Property(e => e.GiaTri).IsRequired().HasMaxLength(255);

                // Quan hệ với SanPham (N-1)
                entity.HasOne(t => t.SanPham)
                      .WithMany() // Nếu bên SanPham chưa có ICollection<ThongSoKyThuat> thì để trống hoặc WithMany(s => s.ThongSoKyThuats) nếu đã thêm
                      .HasForeignKey(t => t.MaSanPham)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --- Cấu hình KhuyenMai ---
            modelBuilder.Entity<KhuyenMai>(e =>
            {
                e.HasIndex(x => x.MaCodeKM).IsUnique(); // Unique Code
                e.Property(x => x.GiaTriGiam).HasColumnType("decimal(18,0)");
                e.Property(x => x.DonHangToiThieu).HasColumnType("decimal(18,0)");
                e.Property(x => x.GiamToiDa).HasColumnType("decimal(18,0)");
            });

            // --- Cấu hình KhuyenMaiKhachHang ---
            modelBuilder.Entity<KhuyenMaiKhachHang>(e =>
            {
                e.HasOne(x => x.KhuyenMai)
                 .WithMany(k => k.KhuyenMaiKhachHangs)
                 .HasForeignKey(x => x.MaKhuyenMai)
                 .OnDelete(DeleteBehavior.Cascade); // Xóa KM thì xóa luôn coupon của khách

                e.HasOne(x => x.KhachHang)
                 .WithMany() // Nếu bên KhachHang chưa có Collection thì để trống
                 .HasForeignKey(x => x.MaKhachHang)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SoDiaChi>(e =>
            {
                e.HasKey(x => x.MaSoDiaChi);
                e.Property(x => x.DiaChiCuThe).IsRequired().HasMaxLength(255);
                // Relationship
                e.HasOne(x => x.KhachHang)
                 .WithMany() // Nếu bên KhachHang có collection thì điền vào
                 .HasForeignKey(x => x.MaKhachHang)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // --- CẤU HÌNH PHIEU NHAP ---
            modelBuilder.Entity<PhieuNhap>(entity =>
            {
                entity.HasKey(e => e.MaPhieuNhap);
                entity.HasIndex(e => e.MaCodePhieu).IsUnique();
                entity.Property(e => e.TongTienNhap).HasColumnType("decimal(18, 0)");

                // Quan hệ với NhanVien
                entity.HasOne(e => e.NhanVien)
                      .WithMany() // Giả sử NhanVien chưa có List<PhieuNhap>, nếu có thì điền vào
                      .HasForeignKey(e => e.MaNhanVienNhap)
                      .OnDelete(DeleteBehavior.Restrict); // Không xóa nhân viên nếu đã có phiếu nhập
            });

            // --- CẤU HÌNH CHI TIET PHIEU NHAP ---
            modelBuilder.Entity<ChiTietPhieuNhap>(entity =>
            {
                entity.HasKey(e => e.MaChiTietPhieuNhap);

                // Quan hệ với PhieuNhap
                entity.HasOne(e => e.PhieuNhap)
                      .WithMany(p => p.ChiTietPhieuNhaps)
                      .HasForeignKey(e => e.MaPhieuNhap)
                      .OnDelete(DeleteBehavior.Cascade); // Xóa phiếu nhập xóa luôn chi tiết

                // Quan hệ với SanPham
                entity.HasOne(e => e.SanPham)
                      .WithMany()
                      .HasForeignKey(e => e.MaSanPham)
                      .OnDelete(DeleteBehavior.Restrict);

                // TRIGGER: Báo cho EF Core biết bảng này có Trigger update kho
                entity.ToTable(tb => tb.HasTrigger("trg_CapNhatTonKho_NhapHang"));
            });

            // --- CẤU HÌNH GIO HANG ---
            modelBuilder.Entity<GioHang>(entity =>
            {
                entity.HasKey(e => e.MaGioHang);
                entity.HasOne(e => e.KhachHang)
                      .WithMany() // 1 KhachHang có thể có logic 1 GioHang (check code)
                      .HasForeignKey(e => e.MaKhachHang)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --- CẤU HÌNH CHI TIET GIO HANG ---
            modelBuilder.Entity<ChiTietGioHang>(entity =>
            {
                entity.HasKey(e => e.MaChiTietGioHang);

                // Quan hệ với GioHang
                entity.HasOne(e => e.GioHang)
                      .WithMany(g => g.ChiTietGioHangs)
                      .HasForeignKey(e => e.MaGioHang)
                      .OnDelete(DeleteBehavior.Cascade); // Xóa giỏ hàng -> Xóa hết item

                // Quan hệ với SanPham
                entity.HasOne(e => e.SanPham)
                      .WithMany()
                      .HasForeignKey(e => e.MaSanPham)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}