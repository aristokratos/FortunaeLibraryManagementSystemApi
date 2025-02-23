
namespace FortunaeLibraryManagementSystem.Domain.Constants;
public static class Message
{
    public const string cachekeyAllBooks = "AllBooks";
    public const string cachekeyAvailableBooks = "AvailableBooks";
    public const string cachekeyTopRatedBooks = "TopRatedBooks";


    public const string UserNotFound = "User not found"; 
    public const string UserNameAlreadyExists = "Username already exists";
    public const string UserDeleted = "User deleted successfully";
    public const string InvalidCredentials = "Invalid credentials";
    public const string BookNotFound = "Book not found";
    public const string RatingNotFound = "Rating not found";
    public const string RatingAlreadyExists = "Rating already exists";
    public const string RatingValueInvalid = "Rating value must be between 1 and 5";
    public const string RatingCommentInvalid = "Rating comment must be less than 500 characters";
    public const string BookAlreadyBorrowed = "Book already borrowed";
    public const string BookNotBorrowed = "Book not borrowed";
    public const string BookAlreadyReturned = "Book already returned";
    public const string BookDeleted = "Book deleted successfully";
    public const string BookAdded = "Book added successfully";
    public const string ImageCannotBeEmpty = "Image cannot be empty";
    public const string BorrowingNotFound = "Borrowing not found";
    public const string InvalidBookId = "Invalid book ID.";
    public const string BookDataCannotBeNull = "Book data cannot be null.";
    public const string BookTitleCannotBeNull = "Book title cannot be null.";
    public const string BookTitleLength = "Book title must be between 1 and 100 characters.";
    public const string BookAuthorLength = "Book author must be less than 100 characters.";
    public const string BookGenreLength = "Book genre must be less than 50 characters.";
    public const string BookDescriptionLength = "Book description must be less than 500 characters.";
    public const string BookISBNLength = "Book ISBN must be less than 50 characters.";
    public const string BookImageLength = "Book image URL must be less than 500 characters.";
    public const string BookAverageRatingInvalid = "Book average rating must be between 0 and 5.";
    public const string BookAlreadyRated = "Book already rated.";
    public const string BookNotRated = "Book not rated.";
    public const string BorrowingDataCannotBeNull = "Borrowing data cannot be null.";
    public const string BorrowingBookIdInvalid = "Borrowing book ID must be valid.";
    public const string BorrowingUserIdInvalid = "Borrowing user ID must be valid.";
    public const string BorrowingBookTitleCannotBeNull = "Borrowing book title cannot be null.";
    public const string BorrowingBookTitleLength = "Borrowing book title must be between 1 and 100 characters.";
    public const string BorrowingPenaltyInvalid = "Borrowing penalty must be greater than or equal to 0.";
    public const string BorrowingAlreadyOverdue = "Borrowing already overdue.";
    public const string BorrowingNotOverdue = "Borrowing not overdue.";
    public const string BorrowingAlreadyReturned = "Borrowing already returned.";
    public const string BorrowingNotReturned = "Borrowing not returned.";
    public const string BorrowingExpectedReturnDateInvalid = "Borrowing expected return date must be greater than or equal to the current date.";
    public const string BorrowingReturnedAtInvalid = "Borrowing returned at date must be greater than or equal to the borrowed at date.";
    public const string BorrowingPenaltyAlreadyPaid = "Borrowing penalty already paid.";
    public const string BorrowingPenaltyNotPaid = "Borrowing penalty not paid.";
    public const string UserAlreadyExists = "User already exists.";
    public const string BookDoesNotExist = "The requested book does not exist.";
    public const string BookNotAvailable = "The requested book is not available for borrowing.";
    public const string BorrowLimimit = "Members can only borrow up to 3 books at a time.";
    public const string BorrowingRecordNotFound = "Borrowing record not found.";
    public const string EmailAlreadyExists = "Email already exists.";
    public const string EmailNotFound = "Email not found.";
    public const string ErrorAddingBook = "An unexpected error occurred while adding the book.";
    public const string ErrorUpdatingBook = "An unexpected error occurred while updating the book.";
    public const string ErrorAddingRating = "An unexpected error occurred while adding the rating.";
    public const string ErrorAddingBorrowing = "An unexpected error occurred while adding the borrowing.";
    public const string ErrorDeletingBook = "An unexpected error occurred while deleting the book.";
    public const string ErrorRetrievingBook = "An unexpected error occurred while retrieving the book.";
}
