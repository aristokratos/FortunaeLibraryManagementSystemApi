
using FortunaeLibraryManagementSystem.Domain.Entities;

namespace FortunaeLibraryManagementSystem.Infrastructure.Interfaces
{
    public interface IRatingRepository
    {
        Task AddRatingAsync(Rating rating);
        Task<List<Rating>> GetRatingsByBookIdAsync(Guid bookId);
        Task<List<Rating>> GetRatingsByUserIdAsync(Guid userId);
    }

}
