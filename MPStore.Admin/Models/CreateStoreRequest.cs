namespace MPStore.Admin.Models.Stores
{
    public class CreateStoreRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? LogoPath { get; set; }

        public bool IsActive { get; set; } = true;
    }
}