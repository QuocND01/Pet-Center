using CloudinaryDotNet.Actions;

namespace FeedbackAPI.Service.Interface
{
    public interface ICloudinaryService
    {
        Task<ImageUploadResult?> UploadImageAsync(IFormFile file, string? folder = null);
        Task<VideoUploadResult?> UploadVideoAsync(IFormFile file, string? folder = null);
        Task<DeletionResult?> DeleteMediaAsync(string publicId, string mediaType = "image");
    }
}
