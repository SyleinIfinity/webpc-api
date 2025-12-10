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
                PublicId = i.PublicId,
                LaAnhDaiDien = i.LaAnhDaiDien // ✅ Bổ sung dòng này
            }).ToList();
        }

        public async Task<ImageResponse> AddImageToProductAsync(int productId, IFormFile file)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null) throw new Exception("Sản phẩm không tồn tại");

            // SỬA ĐOẠN NÀY: Gọi hàm mới
            var uploadResult = await _fileHelper.UploadProductImageAsync(file);

            if (uploadResult == null) throw new Exception("Lỗi upload ảnh lên Cloud");

            var newImage = new HinhAnhSanPham
            {
                MaSanPham = productId,
                UrlHinhAnh = uploadResult.Url,      // .Url
                PublicId = uploadResult.PublicId    // .PublicId
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

        public async Task<bool> DeleteAllImagesByProductIdAsync(int productId)
        {
            // 1. Lấy danh sách ảnh của sản phẩm
            var images = await _imageRepo.GetByProductIdAsync(productId);

            if (images == null || !images.Any())
                return false; // Không có ảnh để xóa

            // 2. Duyệt qua từng ảnh để xóa
            foreach (var img in images)
            {
                // A. Xóa trên Cloudinary (Nếu có PublicId)
                if (!string.IsNullOrEmpty(img.PublicId))
                {
                    try
                    {
                        await _fileHelper.DeleteImageAsync(img.PublicId);
                    }
                    catch (Exception ex)
                    {
                        // Ghi log lỗi xóa cloud nhưng vẫn tiếp tục xóa DB
                        Console.WriteLine($"Lỗi xóa ảnh Cloudinary (ID: {img.PublicId}): {ex.Message}");
                    }
                }

                // B. Xóa trong Database
                // Lưu ý: Nếu Repository của bạn chưa có hàm xóa nhiều (DeleteRange), 
                // ta gọi DeleteAsync từng cái. Với số lượng ảnh ít (<20) thì vẫn ổn.
                await _imageRepo.DeleteAsync(img);
            }

            return true;
        }

        // --- [CẬP NHẬT] SET ẢNH ĐẠI DIỆN THEO SẢN PHẨM ---
        public async Task<bool> SetMainImageAsync(int productId, int imageId)
        {
            // 1. Lấy tất cả ảnh của sản phẩm
            var allImages = await _imageRepo.GetByProductIdAsync(productId);
            var imageList = allImages.ToList(); // Chuyển về List để dễ xử lý

            // Kiểm tra ảnh đích có tồn tại trong danh sách này không
            var targetImage = imageList.FirstOrDefault(x => x.Id == imageId);
            if (targetImage == null)
            {
                throw new Exception($"Hình ảnh ID {imageId} không thuộc về sản phẩm ID {productId}.");
            }

            // 2. Tìm ảnh đang là ảnh đại diện cũ (nếu có) để bỏ đi
            // Chỉ cần update những ảnh đang là true thành false
            var oldMainImages = imageList.Where(x => x.LaAnhDaiDien && x.Id != imageId).ToList();

            foreach (var img in oldMainImages)
            {
                img.LaAnhDaiDien = false;
                await _imageRepo.UpdateAsync(img);
            }

            // 3. Set ảnh mới làm đại diện (nếu nó chưa phải là đại diện)
            if (!targetImage.LaAnhDaiDien)
            {
                targetImage.LaAnhDaiDien = true;
                await _imageRepo.UpdateAsync(targetImage);
            }

            return true;
        }
    }
}