namespace MPStore.Admin.Models.StoreUsers
{
    public class StoreUserDto
    {
        public long Id { get; set; }
        public long StoreId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public string? Email { get; set; }

        public byte Role { get; set; }

        public long? RoleId { get; set; }

        public string? RoleName { get; set; }

        public string? RoleCode { get; set; }

        public string? JobTitle { get; set; }

        public bool IsActive { get; set; }

        public string? ProfileImageUrl { get; set; }

        public string? NationalId { get; set; }

        public string? City { get; set; }
        public string? Country { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? LastLoginAtUtc { get; set; }
    }
}