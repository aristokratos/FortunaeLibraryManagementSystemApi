

namespace FortunaeLibraryManagementSystem.Service.DTOs
{
    public class CreateRatingDTO
    {
        public Guid BookId { get; set; }
        public int Value { get; set; }
        public string? Comment { get; set; }
    }

}
