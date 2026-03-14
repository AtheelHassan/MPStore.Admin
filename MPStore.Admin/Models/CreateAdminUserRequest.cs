namespace MPStore.Admin.Models.AdminUsers
{
    public class CreateAdminUserRequest
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public byte Role { get; set; }

        public bool IsActive { get; set; } = true;
    }
}