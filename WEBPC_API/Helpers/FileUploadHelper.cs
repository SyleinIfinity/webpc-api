using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace WEBPC_API.Helpers
{
    public class CloudinarySettings
    {
        public string CloudName { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
    }

    // Class DTO nhỏ để hứng kết quả trả về
    public class CloudinaryResponse
    {
        public string Url { get; set; }
        public string PublicId { get; set; }
    }

    public class FileUploadHelper
    {
        private readonly Cloudinary _cloudinary;
        private const string FOLDER_AVATAR = "WEBPC/Avatars";
        private const string FOLDER_PRODUCT = "WEBPC/Products";

        public FileUploadHelper(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(acc);
        }

        // 1. Upload Avatar (Giữ nguyên trả về string cho User Service đỡ phải sửa nhiều)
        public async Task<string> UploadAvatarAsync(IFormFile file)
        {
            var result = await UploadFileInternalAsync(file, FOLDER_AVATAR);
            return result?.Url;
        }

        // 2. Upload Sản Phẩm (Trả về Object chứa PublicId để lưu DB)
        public async Task<CloudinaryResponse> UploadProductImageAsync(IFormFile file)
        {
            return await UploadFileInternalAsync(file, FOLDER_PRODUCT);
        }

        // 3. Logic chung
        private async Task<CloudinaryResponse> UploadFileInternalAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0) return null;

            await using var stream = file.OpenReadStream();
            string uniqueFileName = Guid.NewGuid().ToString(); // Dùng GUID cho ngắn gọn, không cần tên gốc

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderName,
                PublicId = uniqueFileName,
                Overwrite = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new CloudinaryResponse
                {
                    Url = uploadResult.SecureUrl.AbsoluteUri,
                    PublicId = uploadResult.PublicId
                };
            }
            return null;
        }

        // 4. Xóa ảnh
        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            return await _cloudinary.DestroyAsync(deleteParams);
        }
    }
}