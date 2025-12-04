using Microsoft.EntityFrameworkCore;
using WEBPC_API.Data;
using WEBPC_API.Models.Interfaces;
using WEBPC_API.Models.Repositories;
using WEBPC_API.Helpers;
using WEBPC_API.Services.Interfaces;
using WEBPC_API.Services.Business;
using System.IO;
using System;
using WEBPC_API.Repositories.Implements;
using WEBPC_API.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Cau hinh DataContext voi chuoi ket noi dong
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("APIConn")));
// 1. Đăng ký Cloudinary Settings
builder.Services.Configure<WEBPC_API.Helpers.CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));
// 1. Cấu hình MailSettings lấy từ appsettings.json
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
// Đăng ký HttpClient cho Helper
builder.Services.AddHttpClient<WEBPC_API.Helpers.LocationHelper>();

// 2. Đăng ký FileUploadHelper
builder.Services.AddScoped<FileUploadHelper>();
// 3. Đăng ký Repository
builder.Services.AddScoped<ISanPhamRepository, SanPhamRepository>();
// 4. Đăng ký Service
builder.Services.AddScoped<ISanPhamService, SanPhamService>();
// Đăng ký Repository Hình Ảnh
builder.Services.AddScoped<IHinhAnhSanPhamRepository, HinhAnhSanPhamRepository>();
// Đăng ký Service Hình Ảnh
builder.Services.AddScoped<IHinhAnhSanPhamService, HinhAnhSanPhamService>();
// Đăng ký Repository
builder.Services.AddScoped<IDanhMucRepository, DanhMucRepository>();
// Đăng ký Service
builder.Services.AddScoped<IDanhMucService, DanhMucService>();
// 2. Đăng ký dịch vụ Mail
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<IOtpService, OtpService>();
// Repositories
builder.Services.AddScoped<IVaiTroRepository, VaiTroRepository>();
builder.Services.AddScoped<ITaiKhoanRepository, TaiKhoanRepository>();
builder.Services.AddScoped<INhanVienRepository, NhanVienRepository>();
builder.Services.AddScoped<IKhachHangRepository, KhachHangRepository>();
// Services
builder.Services.AddScoped<IUserService, UserService>();
// Đăng ký Repository
builder.Services.AddScoped<IThongSoKyThuatRepository, ThongSoKyThuatRepository>();
// Đăng ký Service
builder.Services.AddScoped<IThongSoKyThuatService, ThongSoKyThuatService>();

builder.Services.AddScoped<IKhuyenMaiRepository, KhuyenMaiRepository>();
builder.Services.AddScoped<IKhuyenMaiKhachHangRepository, KhuyenMaiKhachHangRepository>();

// Service
builder.Services.AddScoped<IKhuyenMaiService, KhuyenMaiService>();
builder.Services.AddScoped<IKhuyenMaiKhachHangService, KhuyenMaiKhachHangService>();

builder.Services.AddScoped<ISoDiaChiRepository, SoDiaChiRepository>();
builder.Services.AddScoped<ISoDiaChiService, SoDiaChiService>();

// Đăng ký Repository
builder.Services.AddScoped<IPhieuNhapRepository, PhieuNhapRepository>();
// Đăng ký Service
builder.Services.AddScoped<IPhieuNhapService, PhieuNhapService>();

// Đăng ký Repository
builder.Services.AddScoped<IGioHangRepository, GioHangRepository>();
// Đăng ký Service
builder.Services.AddScoped<IGioHangService, GioHangService>();




// 3. Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 4. Cau hinh CORS (Cho phep WEBPC_WEB goi)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5152") // URL cua du an WEBPC_WEB (MVC)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowWebApp"); // Ap dung CORS

app.UseAuthorization(); // (Them UseAuthentication() neu dung)

app.MapControllers();

app.Run();
