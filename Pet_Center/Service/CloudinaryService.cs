using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ProductAPI.Models;
using ProductAPI.Service.Interface;
using System.Threading.Tasks;


public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> config)
    {
        var settings = config.Value;
        var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    // Upload ảnh
    public async Task<ImageUploadResult> UploadImageAsync(IFormFile file, string folder = null)
    {
        if (file == null || file.Length == 0)
            return null;

        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        return result;
    }

    // Xóa ảnh bằng publicId
    public async Task<DeletionResult> DeleteImageAsync(string publicId)
    {
        if (string.IsNullOrEmpty(publicId))
            return null;

        var deletionParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deletionParams);
        Console.WriteLine($"Deleting: {publicId}");
        Console.WriteLine($"Result: {result.Result}");
        return result;
    }

    // Lấy URL ảnh
    //public string GetImageUrl(string publicId, int width = 0, int height = 0, bool crop = false)
    //{
    //    if (string.IsNullOrEmpty(publicId))
    //        return null;

    //    var transformation = new Transformation();
    //    if (width > 0) transformation.Width(width);
    //    if (height > 0) transformation.Height(height);
    //    if (crop) transformation.Crop("fill");

    //    return _cloudinary.Api.UrlImgUp.Transform(transformation).BuildUrl(publicId);
    //}
}
