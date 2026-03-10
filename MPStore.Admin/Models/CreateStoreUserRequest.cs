namespace MPStore.Admin.Models.StoreUsers
{
    public class CreateStoreUserRequest
    {
        public long StoreId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "Owner";

        public bool IsActive { get; set; } = true;
    }
}