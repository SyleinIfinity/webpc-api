using WEBPC_API.Helpers;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Models.Entities;
using WEBPC_API.Models.Interfaces;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Services.Business
{
    public class SanPhamService : ISanPhamService
    {
        private readonly ISanPhamRepository _repo;
        private readonly FileUploadHelper _fileHelper;

        public SanPhamService(ISanPhamRepository repo, FileUploadHelper fileHelper)
        {
            _repo = repo;
            _fileHelper = fileHelper;
        }

        public async Task<IEnumerable<ProductResponse>> GetAllProductsAsync()
        {
            var products = await _repo.GetAllAsync();
            return products.Select(p => MapToResponse(p));
        }

        public async Task<ProductResponse> GetProductByIdAsync(int id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null) return null;
            return MapToResponse(product);
        }

        public async Task<bool> CreateProductAsync(CreateProductRequest request)
        {
            // 1. Lưu sản phẩm
            var newProduct = new SanPham
            {
                TenSanPham = request.TenSanPham,
                GiaBan = request.GiaBan,
                GiaKhuyenMai = request.GiaKhuyenMai,
                TrangThai = request.TrangThai,
                SoLuongTon = request.SoLuongTon,
                MoTa = request.MoTa,
                MaDanhMuc = request.MaDanhMuc
            };

            int newProductId = await _repo.AddAsync(newProduct);

            // 2. Xử lý ảnh (CODE MỚI)
            if (request.HinhAnhs != null && request.HinhAnhs.Count > 0)
            {
                var imageEntities = new List<HinhAnhSanPham>();

                foreach (var file in request.HinhAnhs)
                {
                    // Gọi hàm mới dành cho Product
                    var uploadResult = await _fileHelper.UploadProductImageAsync(file);

                    if (uploadResult != null)
                    {
                        imageEntities.Add(new HinhAnhSanPham
                        {
                            MaSanPham = newProductId,
                            UrlHinhAnh = uploadResult.Url,      // Lấy từ DTO
                            PublicId = uploadResult.PublicId    // Lấy từ DTO
                        });
                    }
                }
                await _repo.AddImagesAsync(imageEntities);
            }
            return true;
        }

        public async Task<bool> UpdateProductAsync(int id, UpdateProductRequest request)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null) return false;

            // --- LOGIC PATCH: Chỉ cập nhật những trường có dữ liệu ---

            if (!string.IsNullOrEmpty(request.TenSanPham))
                product.TenSanPham = request.TenSanPham;

            if (request.GiaBan.HasValue) // HasValue kiểm tra xem có số hay không
                product.GiaBan = request.GiaBan.Value;

            if (request.GiaKhuyenMai.HasValue)
                product.GiaKhuyenMai = request.GiaKhuyenMai.Value;

            if (request.TrangThai.HasValue)
                product.TrangThai = request.TrangThai.Value;

            if (request.SoLuongTon.HasValue)
                product.SoLuongTon = request.SoLuongTon.Value;

            if (!string.IsNullOrEmpty(request.MoTa))
                product.MoTa = request.MoTa;

            if (request.MaDanhMuc.HasValue)
                product.MaDanhMuc = request.MaDanhMuc.Value;

            // Lưu thông tin cơ bản
            await _repo.UpdateAsync(product);

            // --- XỬ LÝ ẢNH (Logic giữ nguyên nhưng thêm kiểm tra null) ---

            // 1. Xóa ảnh cũ
            if (request.PublicIdsToDelete != null && request.PublicIdsToDelete.Count > 0)
            {
                foreach (var publicId in request.PublicIdsToDelete)
                {
                    await _fileHelper.DeleteImageAsync(publicId);
                }
                await _repo.DeleteImagesAsync(request.PublicIdsToDelete);
            }

            // 2. Thêm ảnh mới
            if (request.HinhAnhs != null && request.HinhAnhs.Count > 0)
            {
                var newImages = new List<HinhAnhSanPham>();
                foreach (var file in request.HinhAnhs)
                {
                    // Gọi hàm mới
                    var uploadResult = await _fileHelper.UploadProductImageAsync(file);

                    if (uploadResult != null)
                    {
                        newImages.Add(new HinhAnhSanPham
                        {
                            MaSanPham = id,
                            UrlHinhAnh = uploadResult.Url,
                            PublicId = uploadResult.PublicId
                        });
                    }
                }
                await _repo.AddImagesAsync(newImages);
            }

            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null) return false;

            // 1. Xóa tất cả ảnh trên Cloudinary trước
            if (product.HinhAnhs != null)
            {
                foreach (var img in product.HinhAnhs)
                {
                    await _fileHelper.DeleteImageAsync(img.PublicId);
                }
            }

            // 2. Xóa sản phẩm trong DB (Cascade sẽ tự xóa HinhAnhSanPham trong DB nếu setup đúng, hoặc Repository xóa)
            await _repo.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<ProductResponse>> GetProductsByCategoryIdAsync(int maDanhMuc)
        {
            var products = await _repo.GetByCategoryIdAsync(maDanhMuc);

            // Map từ Entity sang DTO Response
            return products.Select(p => MapToResponse(p));
        }

        // Hàm phụ trợ để chuyển đổi Entity sang Response DTO
        private ProductResponse MapToResponse(SanPham p)
        {
            return new ProductResponse
            {
                MaSanPham = p.MaSanPham,
                TenSanPham = p.TenSanPham,
                GiaBan = p.GiaBan,
                GiaKhuyenMai = p.GiaKhuyenMai,
                TrangThai = p.TrangThai,
                SoLuongTon = p.SoLuongTon,
                MoTa = p.MoTa,
                TenDanhMuc = p.DanhMuc?.TenDanhMuc,
                DanhSachAnh = p.HinhAnhs?.Select(h => new ImageResponse
                {
                    Id = h.Id,
                    Url = h.UrlHinhAnh,
                    PublicId = h.PublicId
                }).ToList() ?? new List<ImageResponse>()
            };
        }
    }
}