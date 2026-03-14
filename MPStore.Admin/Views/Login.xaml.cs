using MPStore.Admin.Models.Auth;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views
{
    public partial class Login : ContentPage
    {
        private readonly AuthService _authService;
        private bool _isBusy;
        private bool _checkedSavedLogin;
        private bool _navigating;

        public Login(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_isBusy || _navigating || _checkedSavedLogin)
                return;

            _checkedSavedLogin = true;

            try
            {
                var session = await _authService.GetSavedSessionAsync();

                if (session == null || string.IsNullOrWhiteSpace(session.Token) || !session.IsActive)
                {
                    if (session != null && !session.IsActive)
                        await _authService.LogoutAsync();

                    _checkedSavedLogin = false;
                    return;
                }

                _navigating = true;
                await Shell.Current.GoToAsync(AppShell.RouteDashboard);
            }
            catch
            {
                _checkedSavedLogin = false;
                _navigating = false;
            }
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (_isBusy || _navigating)
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

                var savedSession = await _authService.GetSavedSessionAsync();
                if (savedSession == null || string.IsNullOrWhiteSpace(savedSession.Token))
                {
                    ShowMessage("تم تسجيل الدخول لكن تعذر حفظ الجلسة.");
                    return;
                }

                _checkedSavedLogin = true;
                _navigating = true;

                await DisplayAlert("نجاح", "تم تسجيل الدخول بنجاح.", "موافق");
                await Shell.Current.GoToAsync(AppShell.RouteDashboard);
            }
            catch (Exception ex)
            {
                _navigating = false;
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