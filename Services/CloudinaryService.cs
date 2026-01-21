using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ShopNetApi.Exceptions;
using ShopNetApi.Services.Interfaces;

namespace ShopNetApi.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var acc = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );

            _cloudinary = new Cloudinary(acc);
        }

        public async Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new BadRequestException("File không hợp lệ");

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                throw new BadRequestException("Định dạng ảnh không hợp lệ");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "products",
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
                // ❌ Không truyền Transformation
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                throw new InternalServerException("Upload ảnh thất bại");

            return (result.SecureUrl.ToString(), result.PublicId);
        }

        public async Task DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new BadRequestException("PublicId không hợp lệ");

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            if (result.Result != "ok" && result.Result != "not found")
                throw new InternalServerException("Xóa ảnh Cloudinary thất bại");
        }
    }
}
