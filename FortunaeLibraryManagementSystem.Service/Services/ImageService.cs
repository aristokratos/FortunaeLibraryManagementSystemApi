
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
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FortunaeLibraryManagementSystem.Service.DTOs;

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

        private async Task<bool> ValidateBucketAsync()
        {
            try
            {
                var request = new ListBucketsRequest();
                var response = await _s3Client.ListBucketsAsync();

                return response.Buckets.Exists(b => b.BucketName == _bucketName);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Error validating bucket: {Message}", ex.Message);
                throw;
            }
        }

        private async Task CreateBucketIfNotExistsAsync()
        {
            try
            {
                if (!await ValidateBucketAsync())
                {
                    var request = new PutBucketRequest
                    {
                        BucketName = _bucketName,
                        UseClientRegion = true
                    };
                    await _s3Client.PutBucketAsync(request);
                    _logger.LogInformation($"Created new bucket: {_bucketName}");
                }
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Error creating bucket: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<ImageUrlResponseDto> UploadImageAsync(IFormFile file)
        {
            try
            {
                await CreateBucketIfNotExistsAsync();

                // Validate file
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File cannot be null or empty");

                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!AllowedExtensions.Contains(extension))
                    throw new ArgumentException($"Invalid file type. Only {string.Join(", ", AllowedExtensions)} are allowed.");

                // Generate unique filename
                var fileKey = $"books/{Guid.NewGuid():N}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Path.GetFileNameWithoutExtension(file.FileName)}{extension}";

                // Configure upload parameters without public ACL
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileKey,
                    ContentType = file.ContentType,
                    InputStream = file.OpenReadStream(),
                    AutoCloseStream = true
                };

                await _s3Client.PutObjectAsync(request);
                _logger.LogInformation($"Successfully uploaded image to {fileKey}");

                // Generate presigned URL
                var presignedUrl = await GeneratePresignedUrlAsync(fileKey);

                return new ImageUrlResponseDto
                {
                    S3Url = $"https://{_bucketName}.s3.amazonaws.com/{fileKey}",
                    PresignedUrl = presignedUrl
                };
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Detailed error information: {Message}", ex.Message);
                _logger.LogError($"Request ID: {ex.RequestId}");
                throw;
            }
        }

        private async Task<string> GeneratePresignedUrlAsync(string fileKey)
        {
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = fileKey,
                    Verb = HttpVerb.GET,
                    Expires = DateTime.UtcNow.AddHours(24) 
                };

                return _s3Client.GetPreSignedURL(request);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Error generating presigned URL: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> UpdateImageAsync(string fileKey, IFormFile file)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File cannot be null or empty");

                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!AllowedExtensions.Contains(extension))
                    throw new ArgumentException($"Invalid file type. Only {string.Join(", ", AllowedExtensions)} are allowed.");

                // Check if file exists
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = fileKey
                };
                await _s3Client.GetObjectMetadataAsync(request);

                // Upload new version
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileKey,
                    ContentType = file.ContentType,
                    InputStream = file.OpenReadStream(),
                    AutoCloseStream = true
                };

                await _s3Client.PutObjectAsync(putRequest);
                _logger.LogInformation($"Successfully updated image: {fileKey}");

                return true;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Error updating image: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string fileKey)
        {
            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileKey
                };

                await _s3Client.DeleteObjectAsync(request);
                _logger.LogInformation($"Successfully deleted image: {fileKey}");

                return true;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Stream> GetImageAsync(string fileKey)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileKey
                };

                using var response = await _s3Client.GetObjectAsync(request);
                var stream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(stream);
                stream.Position = 0;
                _logger.LogInformation($"Successfully retrieved image: {fileKey}");

                return stream;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Error getting image: {Message}", ex.Message);
                throw;
            }
        }
    }
}