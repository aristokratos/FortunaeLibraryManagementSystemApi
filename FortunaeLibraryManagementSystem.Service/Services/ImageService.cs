
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
using Amazon.S3.Transfer;
using FortunaeLibraryManagementSystem.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
namespace FortunaeLibraryManagementSystem.Service.Services
{
    public class ImageService : IImageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public ImageService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["AWS:S3BucketName"];
        }

        private static readonly HashSet<string> AllowedExtensions = new HashSet<string>
        {
            ".jpg", ".jpeg", ".png", ".gif"
        };

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be null or empty");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Only images are allowed.");

            var fileKey = $"books/{Guid.NewGuid()}_{file.FileName}";

            using (var stream = file.OpenReadStream())
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    Key = fileKey,
                    BucketName = _bucketName,
                    ContentType = file.ContentType,
                    CannedACL = S3CannedACL.PublicRead
                };

                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }

            return $"https://{_bucketName}.s3.amazonaws.com/{fileKey}";
        }

    }
}