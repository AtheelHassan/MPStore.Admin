// File: Models/StoreUsers/StoreUserPermission.cs
namespace MPStore.Admin.Models.StoreUsers
{
    public class StoreUserPermission
    {
        public long Id { get; set; }

        public long StoreUserId { get; set; }

        public string PermissionCode { get; set; } = string.Empty;

        public bool IsAllowed { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}