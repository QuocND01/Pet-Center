using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FeedbackAPI.Models;
using FeedbackAPI.Service.Interface;
using Microsoft.Extensions.Options;

namespace FeedbackAPI.Service
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> config)
        {
            var settings = config.Value;
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<ImageUploadResult?> UploadImageAsync(IFormFile file, string? folder = null)
        {
            if (file == null || file.Length == 0) return null;

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder
            };
            return await _cloudinary.UploadAsync(uploadParams);
        }

        public async Task<VideoUploadResult?> UploadVideoAsync(IFormFile file, string? folder = null)
        {
            if (file == null || file.Length == 0) return null;

            using var stream = file.OpenReadStream();
            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder
            };
            return await _cloudinary.UploadAsync(uploadParams);
        }

        public async Task<DeletionResult?> DeleteMediaAsync(string publicId, string mediaType = "image")
        {
            if (string.IsNullOrEmpty(publicId)) return null;

            var resourceType = mediaType == "video"
                ? ResourceType.Video
                : ResourceType.Image;

            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = resourceType
            };
            return await _cloudinary.DestroyAsync(deletionParams);
        }
    }
}
