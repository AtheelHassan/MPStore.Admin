namespace MPStore.Admin.Models.AdminRoles
{
    public class UpdateAdminRoleRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsSystemRole { get; set; }

        public bool IsActive { get; set; }
    }
}