using WEBPC_API.Helpers;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Models.Entities;
using WEBPC_API.Models.Interfaces;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Services.Business
{
    public class HinhAnhSanPhamService : IHinhAnhSanPhamService
    {
        private readonly IHinhAnhSanPhamRepository _imageRepo;
        private readonly ISanPhamRepository _productRepo; // Cần cái này để check sản phẩm có tồn tại ko
        private readonly FileUploadHelper _fileHelper;

        public HinhAnhSanPhamService(
            IHinhAnhSanPhamRepository imageRepo,
            ISanPhamRepository productRepo,
            FileUploadHelper fileHelper)
        {
            _imageRepo = imageRepo;
            _productRepo = productRepo;
            _fileHelper = fileHelper;
        }

        public async Task<List<ImageResponse>> GetImagesByProductId(int productId)
        {
            var images = await _imageRepo.GetByProductIdAsync(productId);
            return images.Select(i => new ImageResponse
            {
                Id = i.Id,
                Url = i.UrlHinhAnh,
                PublicId = i.PublicId
            }).ToList();
        }

        public async Task<ImageResponse> AddImageToProductAsync(int productId, IFormFile file)
        {
            // 1. Kiểm tra sản phẩm có tồn tại không
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null) throw new Exception("Sản phẩm không tồn tại");

            // 2. Tạo tên file: SP_{ID}_{Random} để tránh trùng
            string customName = $"SP_{productId}_{Guid.NewGuid().ToString().Substring(0, 8)}";

            // 3. Upload lên Cloudinary
            var uploadResult = await _fileHelper.UploadImageAsync(file, "SanPhamPC", customName);

            if (uploadResult == null) throw new Exception("Lỗi upload ảnh lên Cloud");

            // 4. Lưu vào Database
            var newImage = new HinhAnhSanPham
            {
                MaSanPham = productId,
                UrlHinhAnh = uploadResult.SecureUrl.ToString(),
                PublicId = uploadResult.PublicId
            };

            await _imageRepo.AddAsync(newImage);

            return new ImageResponse
            {
                Id = newImage.Id,
                Url = newImage.UrlHinhAnh,
                PublicId = newImage.PublicId
            };
        }

        public async Task<bool> DeleteImageAsync(int imageId)
        {
            // 1. Tìm ảnh trong DB
            var image = await _imageRepo.GetByIdAsync(imageId);
            if (image == null) return false;

            // 2. Xóa trên Cloudinary trước
            if (!string.IsNullOrEmpty(image.PublicId))
            {
                await _fileHelper.DeleteImageAsync(image.PublicId);
            }

            // 3. Xóa trong Database
            await _imageRepo.DeleteAsync(image);
            return true;
        }
    }
}