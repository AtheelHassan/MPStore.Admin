namespace MPStore.Admin.Models.AdminRoles
{
    public class AdminRolePermissionItemRequest
    {
        public string? PermissionCode { get; set; }
        public bool IsAllowed { get; set; }
    }
}