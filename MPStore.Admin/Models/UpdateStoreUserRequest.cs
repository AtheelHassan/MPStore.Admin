namespace MPStore.Admin.Models.StoreUsers
{
    public class UpdateStoreUserRequest
    {
        public long StoreUserId { get; set; }

        public long StoreId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string? Password { get; set; }

        public byte Role { get; set; }

        public bool IsActive { get; set; }
    }
}