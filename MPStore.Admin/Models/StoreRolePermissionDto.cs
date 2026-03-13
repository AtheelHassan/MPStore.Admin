namespace MPStore.Admin.Models.StoreRoles
{
    public class StoreRolePermissionDto
    {
        public long Id { get; set; }
        public long StoreRoleId { get; set; }
        public string PermissionCode { get; set; } = string.Empty;
        public bool IsAllowed { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}