namespace MPStore.Admin.Models.StoreUsers
{
    public class UpdateStoreUserRequest
    {
        public long StoreUserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string? Password { get; set; }

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}