namespace MPStore.Admin.Models.Auth
{
    public class AdminLoginRequest
    {
        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}