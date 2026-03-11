using MPStore.Admin.Models.Auth;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views
{
    public partial class Login : ContentPage
    {
        private readonly AuthService _authService;
        private bool _isBusy;

        public Login(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (_isBusy)
                return;

            MessageLabel.IsVisible = false;
            MessageLabel.Text = string.Empty;

            var userName = UserNameEntry.Text?.Trim() ?? string.Empty;
            var password = PasswordEntry.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userName))
            {
                ShowMessage("يرجى إدخال اسم المستخدم.");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("يرجى إدخال كلمة المرور.");
                return;
            }

            try
            {
                SetBusy(true);

                var request = new AdminLoginRequest
                {
                    Username = userName,
                    Password = password
                };

                var result = await _authService.LoginAsync(request);

                if (result == null || string.IsNullOrWhiteSpace(result.Token))
                {
                    ShowMessage(result?.Message ?? "فشل تسجيل الدخول.");
                    return;
                }

                Preferences.Set("AdminToken", result.Token);

                if (result.Admin != null)
                {
                    Preferences.Set("AdminUserName", result.Admin.Username);
                    Preferences.Set("AdminId", result.Admin.Id);
                }

                var expireAt = DateTime.UtcNow.AddMinutes(result.ExpiresInMinutes);
                Preferences.Set("AdminTokenExpireAt", expireAt.ToString("O"));

                await DisplayAlert("نجاح", "تم تسجيل الدخول بنجاح.", "موافق");

                // الانتقال لاحقاً
                await Shell.Current.GoToAsync("//StoresList");
            }
            catch (Exception ex)
            {
                ShowMessage($"حدث خطأ أثناء تسجيل الدخول: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void SetBusy(bool value)
        {
            _isBusy = value;

            LoadingIndicator.IsVisible = value;
            LoadingIndicator.IsRunning = value;

            LoginButton.IsEnabled = !value;
            UserNameEntry.IsEnabled = !value;
            PasswordEntry.IsEnabled = !value;
        }

        private void ShowMessage(string message)
        {
            MessageLabel.Text = message;
            MessageLabel.IsVisible = true;
        }
    }
}