namespace MPStore.Admin.Models.StoreUsers
{
    public class StoreUserDto
    {
        public long StoreUserId { get; set; }

        public long StoreId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}