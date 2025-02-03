namespace FortunaeLibraryManagementSystem
{
    public class AppSettings
    {
        public class AWSSettings
        {
            public string? AccessKeyId { get; set; }
            public string? SecretAccessKey { get; set; }
            public string? S3BucketName { get; set; }
            public string? Region { get; set; }
        }

        public class CloudinarySettings
        {
            public string? CloudName { get; set; }
            public string? ApiKey { get; set; }
            public string? ApiSecret { get; set; }
        }

        public class JwtSettings
        {
            public string? Issuer { get; set; }
            public string? Audience { get; set; }
            public int ExpirationTime { get; set; }
            public string? SecretKey { get; set; }
        }

        public class SendGridSettings
        {
            public string? ApiKey { get; set; }
            public string? SenderEmail { get; set; }
            public string? SenderName { get; set; }
        }

        public class DatabaseSettings
        {
            public string? ConnectionString { get; set; }
        }
    }
}
