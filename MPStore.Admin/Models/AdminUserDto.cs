namespace MPStore.Admin.Models.AdminUsers
{
    public class AdminUserDto
    {
        public long Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public byte Role { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}