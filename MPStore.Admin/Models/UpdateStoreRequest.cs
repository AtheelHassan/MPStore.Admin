namespace MPStore.Admin.Models.Stores
{
    public class UpdateStoreRequest
    {
        public long StoreId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? LogoPath { get; set; }

        public bool IsActive { get; set; }
    }
}