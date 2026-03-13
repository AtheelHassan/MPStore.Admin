namespace MPStore.Admin.Models.StoreRoles
{
    public class StoreRoleDetailsDto
    {
        public StoreRoleDto? Role { get; set; }
        public List<StoreRolePermissionDto>? Permissions { get; set; }
    }
}