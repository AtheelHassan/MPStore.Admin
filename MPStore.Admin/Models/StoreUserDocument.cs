// File: Models/StoreUsers/StoreUserDocument.cs
namespace MPStore.Admin.Models.StoreUsers
{
    public class StoreUserDocument
    {
        public long Id { get; set; }

        public long StoreUserId { get; set; }

        public string DocumentType { get; set; } = string.Empty;

        public string FileUrl { get; set; } = string.Empty;

        public string? FileName { get; set; }

        public string? ContentType { get; set; }

        public long? FileSizeBytes { get; set; }

        public bool IsPrimary { get; set; }

        public DateTime UploadedAtUtc { get; set; }

        public string? Notes { get; set; }
    }
}