namespace MPStore.Admin.Models.Stores
{
    public class StoreDto
    {
        public long Id { get; set; }

        public long StoreTypeId { get; set; }

        public string? StoreTypeName { get; set; }

        public string? StoreTypeCode { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? LogoPath { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public string? OwnerName { get; set; }

        public string? LegalName { get; set; }

        public string? ShortDescription { get; set; }

        public string? Phone { get; set; }

        public string? WhatsAppNumber { get; set; }

        public string? Email { get; set; }

        public string? WebsiteUrl { get; set; }

        public string? Country { get; set; }

        public string? City { get; set; }

        public string? Region { get; set; }

        public string? AddressLine1 { get; set; }

        public string? AddressLine2 { get; set; }

        public string? PostalCode { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string? CoverImagePath { get; set; }

        public string? TaxNumber { get; set; }

        public string? CommercialRegistrationNo { get; set; }

        public string? WorkingHoursJson { get; set; }

        public string? FacebookUrl { get; set; }

        public string? InstagramUrl { get; set; }

        public string? TelegramUrl { get; set; }

        public string? XUrl { get; set; }

        public bool IsVerified { get; set; }

        public bool IsFeatured { get; set; }

        public DateTime? PublishedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }
    }
}