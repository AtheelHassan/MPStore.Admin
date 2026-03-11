using MPStore.Admin.Models.StoreUsers;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views
{
    [QueryProperty(nameof(StoreId), "storeId")]
    public partial class AddStoreUser : ContentPage
    {
        private readonly StoreUsersService _storeUsersService;
        private bool _isBusy;
        private long _storeId;

        public string? StoreId
        {
            get => _storeId.ToString();
            set
            {
                if (long.TryParse(value, out var id))
                    _storeId = id;
            }
        }

        public AddStoreUser(StoreUsersService storeUsersService)
        {
            InitializeComponent();
            _storeUsersService = storeUsersService;
            RolePicker.SelectedIndex = 0;
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

            if (RolePicker.SelectedIndex < 0)
            {
                ShowMessage("يرجى اختيار الدور.");
                return;
            }

            try
            {
                SetBusy(true);

                var request = new CreateStoreUserRequest
                {
                    StoreId = _storeId,
                    FullName = fullName,
                    Phone = phone,
                    Password = password,
                    Role = GetRoleValue(RolePicker.SelectedIndex),
                    IsActive = IsActiveSwitch.IsToggled
                };

                var result = await _storeUsersService.CreateStoreUserAsync(request);

                if (!result)
                {
                    ShowMessage("فشل حفظ العامل.");
                    return;
                }

                await DisplayAlert("نجاح", "تم إنشاء العامل بنجاح.", "موافق");
                await Shell.Current.GoToAsync("..");
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
            await Shell.Current.GoToAsync($"{nameof(StoreUsersList)}?storeId={_storeId}");
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

        private static byte GetRoleValue(int selectedIndex)
        {
            return selectedIndex switch
            {
                0 => 1,
                1 => 2,
                _ => 3
            };
        }
    }
}