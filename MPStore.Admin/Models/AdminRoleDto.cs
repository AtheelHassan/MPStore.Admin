namespace MPStore.Admin.Models.AdminRoles
{
    public class AdminRoleDto
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? Description { get; set; }

        public bool IsSystemRole { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}