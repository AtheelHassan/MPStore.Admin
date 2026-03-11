namespace MPStore.Admin.Models.StoreUsers
{
    public class StoreUserDto
    {
        public long Id { get; set; }
        public long StoreId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public byte Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}