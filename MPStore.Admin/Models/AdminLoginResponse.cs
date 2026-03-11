namespace MPStore.Admin.Models.Auth
{
    public class AdminLoginResponse
    {
        public string Message { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public int ExpiresInMinutes { get; set; }

        public AdminDto? Admin { get; set; }
    }

    public class AdminDto
    {
        public long Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public byte Role { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}