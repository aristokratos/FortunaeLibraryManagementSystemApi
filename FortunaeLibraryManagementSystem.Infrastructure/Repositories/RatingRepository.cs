

using FortunaeLibraryManagementSystem.Domain.Entities;
using FortunaeLibraryManagementSystem.Infrastructure.Data;
using FortunaeLibraryManagementSystem.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FortunaeLibraryManagementSystem.Infrastructure.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly LibraryDbContext _context;

        public RatingRepository(LibraryDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new rating to the database.
        /// </summary>
        public async Task AddRatingAsync(Rating rating)
        {
            if (rating == null) throw new ArgumentNullException(nameof(rating));

            await _context.Ratings.AddAsync(rating);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves all ratings for a specific book by its ID.
        /// </summary>
        public async Task<List<Rating>> GetRatingsByBookIdAsync(Guid bookId)
        {
            return await _context.Ratings
                .Where(r => r.BookId == bookId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all ratings made by a specific user by their ID.
        /// </summary>
        public async Task<List<Rating>> GetRatingsByUserIdAsync(Guid userId)
        {
            return await _context.Ratings
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }

}
