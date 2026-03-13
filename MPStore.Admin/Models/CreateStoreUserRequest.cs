namespace MPStore.Admin.Models.StoreUsers
{
    public class CreateStoreUserRequest
    {
        public long StoreId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public byte Role { get; set; }

        public long? RoleId { get; set; }

        public bool IsActive { get; set; }
    }
}