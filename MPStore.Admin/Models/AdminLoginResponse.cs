namespace MPStore.Admin.Models.Auth
{
    public class AdminLoginResponse
    {
        public long AdminId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public DateTime ExpireAt { get; set; }
    }
}