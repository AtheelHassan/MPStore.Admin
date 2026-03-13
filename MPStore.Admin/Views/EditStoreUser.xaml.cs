using MPStore.Admin.Models.StoreRoles;
using MPStore.Admin.Models.StoreUsers;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views
{
    [QueryProperty(nameof(UserId), "userId")]
    [QueryProperty(nameof(StoreId), "storeId")]
    public partial class EditStoreUser : ContentPage
    {
        private readonly StoreUsersService _service;
        private readonly StoreRolesService _storeRolesService;
        private bool _isBusy;
        private bool _rolesLoaded;

        private long _userId;
        private long _storeId;

        private readonly List<StoreRoleDto> _roles = new();

        public string? UserId
        {
            get => _userId.ToString();
            set
            {
                if (long.TryParse(value, out var id))
                    _userId = id;
            }
        }

        public string? StoreId
        {
            get => _storeId.ToString();
            set
            {
                if (long.TryParse(value, out var id))
                    _storeId = id;
            }
        }

        public EditStoreUser(StoreUsersService service, StoreRolesService storeRolesService)
        {
            InitializeComponent();
            _service = service;
            _storeRolesService = storeRolesService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_storeId <= 0 || _userId <= 0)
                return;

            await LoadRolesAndUserAsync();
        }

        private async Task LoadRolesAndUserAsync()
        {
            if (_isBusy)
                return;

            try
            {
                SetBusy(true);
                MessageLabel.IsVisible = false;

                if (!_rolesLoaded)
                    await LoadRolesAsync();

                await LoadUserAsync();
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

        private async Task LoadRolesAsync()
        {
            _roles.Clear();
            RolePicker.ItemsSource = null;
            RolePicker.SelectedIndex = -1;

            var roles = await _storeRolesService.GetStoreRolesAsync(_storeId);

            if (roles == null || roles.Count == 0)
            {
                ShowMessage("لا توجد أدوار متاحة لهذا المتجر.");
                return;
            }

            _roles.AddRange(roles.Where(x => x.IsActive));

            RolePicker.ItemsSource = _roles.Select(x => x.Name).ToList();
            _rolesLoaded = true;
        }

        private async Task LoadUserAsync()
        {
            if (_userId <= 0)
                return;

            var result = await _service.GetStoreUserAsync(_userId);
            var user = result?.User;

            if (user == null)
            {
                ShowMessage("لم يتم العثور على العامل.");
                return;
            }

            FullNameEntry.Text = user.FullName;
            PhoneEntry.Text = user.Phone;

            var selectedIndex = -1;

            if (user.RoleId > 0)
            {
                selectedIndex = _roles.FindIndex(x => x.Id == user.RoleId);
            }

            if (selectedIndex < 0 && !string.IsNullOrWhiteSpace(user.RoleCode))
            {
                selectedIndex = _roles.FindIndex(x =>
                    string.Equals(x.Code, user.RoleCode, StringComparison.OrdinalIgnoreCase));
            }

            if (selectedIndex < 0 && !string.IsNullOrWhiteSpace(user.RoleName))
            {
                selectedIndex = _roles.FindIndex(x =>
                    string.Equals(x.Name, user.RoleName, StringComparison.OrdinalIgnoreCase));
            }

            RolePicker.SelectedIndex = selectedIndex >= 0 ? selectedIndex : -1;

            IsActiveSwitch.IsToggled = user.IsActive;
            IsActiveTextLabel.Text = user.IsActive ? "العامل نشط" : "العامل متوقف";
        }

        private void OnIsActiveToggled(object sender, ToggledEventArgs e)
        {
            IsActiveTextLabel.Text = e.Value ? "العامل نشط" : "العامل متوقف";
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_isBusy)
                return;

            MessageLabel.IsVisible = false;

            var fullName = FullNameEntry.Text?.Trim() ?? string.Empty;
            var phone = PhoneEntry.Text?.Trim() ?? string.Empty;
            var password = PasswordEntry.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(fullName))
            {
                ShowMessage("يرجى إدخال الاسم الكامل.");
                return;
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                ShowMessage("يرجى إدخال رقم الهاتف.");
                return;
            }

            if (RolePicker.SelectedIndex < 0 || RolePicker.SelectedIndex >= _roles.Count)
            {
                ShowMessage("يرجى اختيار الدور.");
                return;
            }

            try
            {
                SetBusy(true);

                var selectedRole = _roles[RolePicker.SelectedIndex];

                var request = new UpdateStoreUserRequest
                {
                    StoreUserId = _userId,
                    StoreId = _storeId,
                    FullName = fullName,
                    Phone = phone,
                    Password = string.IsNullOrWhiteSpace(password) ? null : password,
                    RoleId = selectedRole.Id,
                    RoleCode = selectedRole.Code,
                    RoleName = selectedRole.Name,
                    IsActive = IsActiveSwitch.IsToggled
                };

                var updateResult = await _service.UpdateStoreUserAsync(request);

                if (!updateResult)
                {
                    ShowMessage("فشل تحديث العامل.");
                    return;
                }

                await DisplayAlert("نجاح", "تم تحديث بيانات العامل.", "موافق");
                await Shell.Current.GoToAsync($"{AppShell.RouteStoreUsersList}?storeId={_storeId}");
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
            await Shell.Current.GoToAsync($"{AppShell.RouteStoreUsersList}?storeId={_storeId}");
        }

        private void SetBusy(bool value)
        {
            _isBusy = value;

            LoadingIndicator.IsVisible = value;
            LoadingIndicator.IsRunning = value;

            SaveButton.IsEnabled = !value;
            FullNameEntry.IsEnabled = !value;
            PhoneEntry.IsEnabled = !value;
            PasswordEntry.IsEnabled = !value;
            RolePicker.IsEnabled = !value;
            IsActiveSwitch.IsEnabled = !value;
        }

        private void ShowMessage(string message)
        {
            MessageLabel.Text = message;
            MessageLabel.IsVisible = true;
        }
    }
}