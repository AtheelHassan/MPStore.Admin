using MPStore.Admin.Models.StoreUsers;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views
{
    [QueryProperty(nameof(UserId), "userId")]
    [QueryProperty(nameof(StoreId), "storeId")]
    public partial class EditStoreUser : ContentPage
    {
        private readonly StoreUsersService _service;
        private bool _isBusy;

        private long _userId;
        private long _storeId;

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

        public EditStoreUser(StoreUsersService service)
        {
            InitializeComponent();
            _service = service;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadUser();
        }

        private async Task LoadUser()
        {
            if (_isBusy || _userId <= 0)
                return;

            try
            {
                SetBusy(true);

                var user = await _service.GetStoreUserAsync(_userId);

                if (user == null)
                {
                    ShowMessage("لم يتم العثور على العامل.");
                    return;
                }

                FullNameEntry.Text = user.FullName;
                PhoneEntry.Text = user.Phone;

                RolePicker.SelectedIndex = user.Role switch
                {
                    1 => 0,
                    2 => 1,
                    _ => 2
                };

                IsActiveSwitch.IsToggled = user.IsActive;
                IsActiveTextLabel.Text = user.IsActive ? "العامل نشط" : "العامل متوقف";
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

            if (RolePicker.SelectedIndex < 0)
            {
                ShowMessage("يرجى اختيار الدور.");
                return;
            }

            try
            {
                SetBusy(true);

                var request = new UpdateStoreUserRequest
                {
                    StoreUserId = _userId,
                    StoreId = _storeId,
                    FullName = fullName,
                    Phone = phone,
                    Password = string.IsNullOrWhiteSpace(password) ? null : password,
                    Role = GetRoleValue(RolePicker.SelectedIndex),
                    IsActive = IsActiveSwitch.IsToggled
                };

                var result = await _service.UpdateStoreUserAsync(request);

                if (!result)
                {
                    ShowMessage("فشل تحديث العامل.");
                    return;
                }

                await DisplayAlert("نجاح", "تم تحديث بيانات العامل.", "موافق");
                await Shell.Current.GoToAsync($"{nameof(StoreUsersList)}?storeId={_storeId}");
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