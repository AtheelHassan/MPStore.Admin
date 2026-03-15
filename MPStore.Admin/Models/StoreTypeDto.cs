namespace MPStore.Admin.Models.Stores
{
    public class StoreTypeDto
    {
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public int SortOrder { get; set; }
    }
}