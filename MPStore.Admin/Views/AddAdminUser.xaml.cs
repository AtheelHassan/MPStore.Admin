using MPStore.Admin.Models.AdminUsers;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views
{
    public partial class AddAdminUser : ContentPage
    {
        private readonly AdminUsersService _adminUsersService;
        private readonly AuthService _authService;

        private bool _isBusy;
        private bool _canCreateAdminUser;

        public AddAdminUser(
            AdminUsersService adminUsersService,
            AuthService authService)
        {
            InitializeComponent();
            _adminUsersService = adminUsersService;
            _authService = authService;

            RolePicker.SelectedIndex = 0;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPermissionAsync();
        }

        private async Task LoadPermissionAsync()
        {
            try
            {
                _canCreateAdminUser = await _authService.HasPermissionAsync("admin_users.create");
            }
            catch
            {
                _canCreateAdminUser = false;
            }

            SaveButton.IsVisible = _canCreateAdminUser;

            if (!_canCreateAdminUser)
            {
                SetEditorsEnabled(false);
                ShowMessage("ليس لديك صلاحية لإضافة أدمن.");
            }
            else
            {
                SetEditorsEnabled(!_isBusy);
                MessageLabel.IsVisible = false;
            }
        }

        private void OnIsActiveToggled(object sender, ToggledEventArgs e)
        {
            IsActiveTextLabel.Text = e.Value ? "الأدمن نشط" : "الأدمن متوقف";
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_isBusy || !_canCreateAdminUser)
                return;

            MessageLabel.IsVisible = false;

            var username = UsernameEntry.Text?.Trim() ?? string.Empty;
            var password = PasswordEntry.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowMessage("يرجى إدخال اسم المستخدم.");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("يرجى إدخال كلمة المرور.");
                return;
            }

            if (RolePicker.SelectedIndex < 0)
            {
                ShowMessage("يرجى اختيار الدور الإداري.");
                return;
            }

            try
            {
                SetBusy(true);

                var request = new CreateAdminUserRequest
                {
                    Username = username,
                    Password = password,
                    Role = MapRoleFromPicker(RolePicker.SelectedIndex),
                    IsActive = IsActiveSwitch.IsToggled
                };

                var result = await _adminUsersService.CreateAsync(request);

                if (!result)
                {
                    ShowMessage("فشل حفظ الأدمن.");
                    return;
                }

                await DisplayAlert("نجاح", "تم إنشاء الأدمن بنجاح.", "موافق");
                await Shell.Current.GoToAsync(AppShell.RouteAdminUsersList);
            }
            catch (Exception ex)
            {
                ShowMessage($"خطأ: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private static byte MapRoleFromPicker(int selectedIndex)
        {
            return selectedIndex switch
            {
                0 => 1, // مدير عام
                1 => 2, // أدمن
                2 => 3, // مشرف
                _ => 2
            };
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(AppShell.RouteAdminUsersList);
        }

        private void SetBusy(bool value)
        {
            _isBusy = value;

            LoadingIndicator.IsVisible = value;
            LoadingIndicator.IsRunning = value;

            SaveButton.IsEnabled = _canCreateAdminUser && !value;
            SetEditorsEnabled(_canCreateAdminUser && !value);
        }

        private void SetEditorsEnabled(bool value)
        {
            UsernameEntry.IsEnabled = value;
            PasswordEntry.IsEnabled = value;
            RolePicker.IsEnabled = value;
            IsActiveSwitch.IsEnabled = value;
        }

        private void ShowMessage(string message)
        {
            MessageLabel.Text = message;
            MessageLabel.IsVisible = true;
        }
    }
}