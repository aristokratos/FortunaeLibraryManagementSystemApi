

using FortunaeLibraryManagementSystem.Service.DTOs;

namespace FortunaeLibraryManagementSystem.Service.Interfaces
{
    public interface IBorrowingService
    {
        Task<BorrowingDTO> BorrowBookAsync(Guid userId, BorrowBookDTO borrowBookDto);
        Task ReturnBookAsync(Guid borrowingId);
        Task<List<BorrowingDTO>> GetMemberBorrowingHistoryAsync(Guid userId);
        Task<List<BorrowingDTO>> GetActiveBorrowingsAsync(Guid userId);
        Task<List<BorrowingDTO>> GetAllBorrowingsAsync();
        Task PenalizeMemberAsync(Guid borrowingId, decimal penalty);
        Task MarkBookAsReturnedAsync(Guid borrowingId);
    }
}
