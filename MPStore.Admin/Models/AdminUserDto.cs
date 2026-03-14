namespace MPStore.Admin.Models.AdminUsers
{
    public class AdminUserDto
    {
        public long Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public byte Role { get; set; }

        public bool IsActive { get; set; }

        public long? AdminRoleId { get; set; }

        public string? DisplayName { get; set; }

        public string? Email { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? LastLoginAtUtc { get; set; }
    }
}