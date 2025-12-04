using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace WEBPC_API.Helpers
{
    // Class để map dữ liệu từ appsettings.json
    public class CloudinarySettings
    {
        public string CloudName { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
    }

    public class FileUploadHelper
    {
        private readonly Cloudinary _cloudinary;

        public FileUploadHelper(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(acc);
        }

        // Hàm Upload ảnh
        // file: File ảnh từ form
        // folderName: Tên thư mục trên Cloud (ví dụ: "SanPham")
        // customFileName: Tên file muốn đặt (ví dụ: "liems01"), nếu null sẽ lấy tên gốc
        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file, string folderName, string customFileName = null)
        {
            if (file.Length > 0)
            {
                await using var stream = file.OpenReadStream();

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folderName,
                    // Nếu có tên tùy chỉnh thì dùng, không thì để Cloudinary tự sinh ngẫu nhiên hoặc dùng tên gốc
                    PublicId = customFileName ?? Path.GetFileNameWithoutExtension(file.FileName),
                    Overwrite = true // Cho phép ghi đè nếu trùng tên
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                return uploadResult;
            }
            return null;
        }

        // Hàm Xóa ảnh
        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            return await _cloudinary.DestroyAsync(deleteParams);
        }
    }
}