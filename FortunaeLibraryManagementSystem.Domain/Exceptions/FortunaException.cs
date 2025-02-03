namespace FortunaeLibraryManagementSystem.Domain.Exceptions
{
    public class FortunaException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }

        public FortunaException(string message, int statusCode = 500, string errorCode = "INTERNAL_ERROR")
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        public FortunaException(string message, Exception innerException, int statusCode = 500, string errorCode = "INTERNAL_ERROR")
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }

    public class NotFoundException : FortunaException
    {
        public NotFoundException(string message)
            : base(message, 404, "NOT_FOUND")
        {
        }
    }

    public class BadRequestException : FortunaException
    {
        public BadRequestException(string message)
            : base(message, 400, "BAD_REQUEST")
        {
        }
    }

    public class UnauthorizedException : FortunaException
    {
        public UnauthorizedException(string message)
            : base(message, 401, "UNAUTHORIZED")
        {
        }
    }

    public class ForbiddenException : FortunaException
    {
        public ForbiddenException(string message)
            : base(message, 403, "FORBIDDEN")
        {
        }
    }

    public class ValidationException : FortunaException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("One or more validation failures have occurred.", 400, "VALIDATION_ERROR")
        {
            Errors = errors;
        }
    }

    public class ConflictException : FortunaException
    {
        public ConflictException(string message)
            : base(message, 409, "CONFLICT")
        {
        }
    }
}