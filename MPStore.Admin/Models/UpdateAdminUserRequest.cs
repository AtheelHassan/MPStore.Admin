namespace MPStore.Admin.Models.AdminUsers
{
    public class UpdateAdminUserRequest
    {
        public string Username { get; set; } = string.Empty;

        public byte Role { get; set; }

        public bool IsActive { get; set; }

        public long? AdminRoleId { get; set; }

        public string? DisplayName { get; set; }

        public string? Email { get; set; }
    }
}