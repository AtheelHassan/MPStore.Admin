using MPStore.Admin.Models.AdminUsers;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views
{
    [QueryProperty(nameof(UserId), "userId")]
    public partial class EditAdminUser : ContentPage
    {
        private readonly AdminUsersService _adminUsersService;
        private readonly AuthService _authService;

        private bool _isBusy;
        private bool _canUpdateAdminUser;
        private long _userId;

        public string? UserId
        {
            get => _userId.ToString();
            set
            {
                if (long.TryParse(value, out var id))
                    _userId = id;
            }
        }

        public EditAdminUser(
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

            if (_userId <= 0)
                return;

            await LoadPermissionAsync();

            if (!_canUpdateAdminUser)
                return;

            await LoadAdminUserAsync();
        }

        private async Task LoadPermissionAsync()
        {
            try
            {
                _canUpdateAdminUser = await _authService.HasPermissionAsync("admin_users.update");
            }
            catch
            {
                _canUpdateAdminUser = false;
            }

            SaveButton.IsVisible = _canUpdateAdminUser;

            if (!_canUpdateAdminUser)
            {
                SetEditorsEnabled(false);
                ShowMessage("ليس لديك صلاحية لتعديل الأدمن.");
            }
            else
            {
                SetEditorsEnabled(!_isBusy);
                MessageLabel.IsVisible = false;
            }
        }

        private async Task LoadAdminUserAsync()
        {
            if (_isBusy || _userId <= 0)
                return;

            try
            {
                SetBusy(true);
                MessageLabel.IsVisible = false;

                var user = await _adminUsersService.GetByIdAsync(_userId);

                if (user == null)
                {
                    ShowMessage("لم يتم العثور على الأدمن.");
                    return;
                }

                UsernameEntry.Text = user.Username ?? string.Empty;
                RolePicker.SelectedIndex = user.Role > 0 ? user.Role - 1 : 0;
                IsActiveSwitch.IsToggled = user.IsActive;
                IsActiveTextLabel.Text = user.IsActive ? "الأدمن نشط" : "الأدمن متوقف";
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

        private void OnIsActiveToggled(object sender, ToggledEventArgs e)
        {
            IsActiveTextLabel.Text = e.Value ? "الأدمن نشط" : "الأدمن متوقف";
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_isBusy || !_canUpdateAdminUser)
                return;

            MessageLabel.IsVisible = false;

            var username = UsernameEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowMessage("يرجى إدخال اسم المستخدم.");
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

                var request = new UpdateAdminUserRequest
                {
                    Username = username,
                    Role = (byte)(RolePicker.SelectedIndex + 1),
                    IsActive = IsActiveSwitch.IsToggled
                };

                var result = await _adminUsersService.UpdateAsync(_userId, request);

                if (!result)
                {
                    ShowMessage("فشل تحديث الأدمن.");
                    return;
                }

                await DisplayAlert("نجاح", "تم تحديث بيانات الأدمن بنجاح.", "موافق");
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

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(AppShell.RouteAdminUsersList);
        }

        private void SetBusy(bool value)
        {
            _isBusy = value;

            LoadingIndicator.IsVisible = value;
            LoadingIndicator.IsRunning = value;

            SaveButton.IsEnabled = _canUpdateAdminUser && !value;
            SetEditorsEnabled(_canUpdateAdminUser && !value);
        }

        private void SetEditorsEnabled(bool value)
        {
            UsernameEntry.IsEnabled = value;
            PasswordEntry.IsEnabled = false;
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