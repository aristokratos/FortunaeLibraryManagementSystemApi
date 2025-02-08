
#region cloudinary
//namespace FortunaeLibraryManagementSystem.Service.Services;
//using CloudinaryDotNet;
//using CloudinaryDotNet.Actions;
//using FortunaeLibraryManagementSystem.Service.Interfaces;
//using Microsoft.AspNetCore.Http;

//public class ImageService : IImageService
//{
//    private readonly Cloudinary _cloudinary;

//    public ImageService(Cloudinary cloudinary)
//    {
//        _cloudinary = cloudinary;
//    }

//    public async Task<string> UploadImageAsync(IFormFile file)
//    {
//        using (var stream = file.OpenReadStream())
//        {
//            var uploadParams = new ImageUploadParams
//            {
//                File = new FileDescription(file.FileName, stream),
//                Folder = "books",
//                PublicId = Guid.NewGuid().ToString()
//            };

//            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

//            if (uploadResult.Error != null)
//            {
//                throw new Exception($"Cloudinary error: {uploadResult.Error.Message}");
//            }

//            return uploadResult.SecureUrl.ToString();
//        }
//    }
//}

#endregion cloudinary

using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using FortunaeLibraryManagementSystem.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
namespace FortunaeLibraryManagementSystem.Service.Services
{
    public class ImageService : IImageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IAmazonS3 s3Client, IConfiguration configuration, ILogger<ImageService> logger)
        {
            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
            _bucketName = configuration["AWS:S3BucketName"] ?? throw new ArgumentException("S3 bucket name not found in configuration");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private static readonly HashSet<string> AllowedExtensions = new HashSet<string>
    {
        ".jpg", ".jpeg", ".png", ".gif",
        ".bmp", ".tiff", ".tif"
    };

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File cannot be null or empty");

                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!AllowedExtensions.Contains(extension))
                    throw new ArgumentException($"Invalid file type. Only {string.Join(", ", AllowedExtensions)} are allowed.");

                // Generate unique filename
                var fileKey = $"books/{Guid.NewGuid():N}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Path.GetFileNameWithoutExtension(file.FileName)}{extension}";

                // Configure upload parameters
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileKey,
                    ContentType = file.ContentType,
                    InputStream = file.OpenReadStream(),
                    AutoCloseStream = true,
                    CannedACL = S3CannedACL.PublicRead
                };

                // Upload with retry logic
                await _s3Client.PutObjectAsync(request);

                // Verify upload
                var response = await _s3Client.GetObjectMetadataAsync(_bucketName, fileKey);
                if (response.HttpStatusCode != HttpStatusCode.OK)
                    throw new Exception($"Failed to verify uploaded object metadata");

                return $"https://{_bucketName}.s3.amazonaws.com/{fileKey}";
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to S3: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during image upload: {Message}", ex.Message);
                throw;
            }
        }
    }
}