
using Microsoft.AspNetCore.Http;

namespace FortunaeLibraryManagementSystem.Service.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file);
    }
}
