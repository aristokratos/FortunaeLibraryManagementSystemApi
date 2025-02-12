
using FortunaeLibraryManagementSystem.Service.DTOs;
using Microsoft.AspNetCore.Http;

namespace FortunaeLibraryManagementSystem.Service.Interfaces;

public interface IImageService
{
    Task<ImageUrlResponseDto> UploadImageAsync(IFormFile file);
    Task<bool> UpdateImageAsync(string fileKey, IFormFile file);
    Task<bool> DeleteImageAsync(string fileKey);
    Task<Stream> GetImageAsync(string fileKey);
}
