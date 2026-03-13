namespace MPStore.Admin.Models.StoreRoles
{
    public class StoreRoleDto
    {
        public long Id { get; set; }
        public long StoreId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int PermissionsCount { get; set; }
    }
}