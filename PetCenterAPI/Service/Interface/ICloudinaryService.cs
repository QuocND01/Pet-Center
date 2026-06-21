using CloudinaryDotNet.Actions;

namespace PetCenterAPI.Service.Interface
{
    public interface ICloudinaryService
    {
        Task<ImageUploadResult> UploadImageAsync(IFormFile file, string folder = null);
        Task<DeletionResult> DeleteImageAsync(string publicId);

        //string GetImageUrl(string publicId, int width = 0, int height = 0, bool crop = false);

        Task<VideoUploadResult?> UploadVideoAsync(IFormFile file, string? folder = null);
        Task DeleteMediaAsync(string publicId, string mediaType);
    }
}
