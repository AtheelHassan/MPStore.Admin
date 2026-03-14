namespace MPStore.Admin.Models.Auth
{
    public class AdminSession
    {
        public string Token { get; set; } = string.Empty;

        public int ExpiresInMinutes { get; set; }

        public long Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        public string? Email { get; set; }

        public byte Role { get; set; }

        public long? AdminRoleId { get; set; }

        public string? AdminRoleName { get; set; }

        public string? AdminRoleCode { get; set; }

        public List<string> Permissions { get; set; } = new();

        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? LastLoginAtUtc { get; set; }
    }
}