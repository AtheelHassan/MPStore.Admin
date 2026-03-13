using MPStore.Admin.Models.StoreRoles;
using MPStore.Admin.Models.StoreUsers;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views
{
    [QueryProperty(nameof(StoreId), "storeId")]
    public partial class AddStoreUser : ContentPage
    {
        private readonly StoreUsersService _storeUsersService;
        private readonly StoreRolesService _storeRolesService;
        private bool _isBusy;
        private bool _rolesLoaded;
        private long _storeId;

        private readonly List<StoreRoleDto> _roles = new();

        public string? StoreId
        {
            get => _storeId.ToString();
            set
            {
                if (long.TryParse(value, out var id))
                    _storeId = id;
            }
        }

        public AddStoreUser(StoreUsersService storeUsersService, StoreRolesService storeRolesService)
        {
            InitializeComponent();
            _storeUsersService = storeUsersService;
            _storeRolesService = storeRolesService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_storeId <= 0 || _rolesLoaded)
                return;

            await LoadRolesAsync();
        }

        private async Task LoadRolesAsync()
        {
            if (_isBusy)
                return;

            try
            {
                SetBusy(true);
                MessageLabel.IsVisible = false;

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

                if (_roles.Count == 0)
                {
                    ShowMessage("لا توجد أدوار نشطة لهذا المتجر.");
                    return;
                }

                RolePicker.ItemsSource = _roles.Select(x => x.Name).ToList();
                RolePicker.SelectedIndex = 0;
                _rolesLoaded = true;
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

            if (_storeId <= 0)
            {
                ShowMessage("رقم المتجر غير صالح.");
                return;
            }

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

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("يرجى إدخال كلمة المرور.");
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

                var request = new CreateStoreUserRequest
                {
                    StoreId = _storeId,
                    FullName = fullName,
                    Phone = phone,
                    Password = password,
                    Role = GetLegacyRoleValue(selectedRole.Code),
                    RoleId = selectedRole.Id,
                    IsActive = IsActiveSwitch.IsToggled
                };

                var result = await _storeUsersService.CreateStoreUserAsync(request);

                if (!result)
                {
                    ShowMessage("فشل حفظ العامل.");
                    return;
                }

                await DisplayAlert("نجاح", "تم إنشاء العامل بنجاح.", "موافق");
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

        private static byte GetLegacyRoleValue(string? roleCode)
        {
            return (roleCode ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "owner" => 1,
                "manager" => 2,
                _ => 3
            };
        }
    }
}