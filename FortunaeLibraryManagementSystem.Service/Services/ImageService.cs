

namespace FortunaeLibraryManagementSystem.Service.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FortunaeLibraryManagementSystem.Service.Interfaces;
using Microsoft.AspNetCore.Http;

public class ImageService : IImageService
{
    private readonly Cloudinary _cloudinary;

    public ImageService(Cloudinary cloudinary)
    {
        _cloudinary = cloudinary;
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        using (var stream = file.OpenReadStream())
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "books",
                PublicId = Guid.NewGuid().ToString()
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception($"Cloudinary error: {uploadResult.Error.Message}");
            }

            return uploadResult.SecureUrl.ToString();
        }
    }
}


