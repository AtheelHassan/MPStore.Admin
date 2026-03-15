using MPStore.Admin.Models.Stores;
using MPStore.Admin.Services;
using Microsoft.Maui.Storage;

namespace MPStore.Admin.Views
{
    public partial class AddStore : ContentPage
    {
        private readonly StoresService _storesService;
        private readonly AuthService _authService;
        private bool _isBusy;
        private bool _canCreateStore;

        private string? _selectedImagePath;
        private List<StoreTypeDto> _storeTypes = new();

        public AddStore(StoresService storesService, AuthService authService)
        {
            InitializeComponent();
            _storesService = storesService;
            _authService = authService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPermissionAsync();
            await LoadStoreTypesAsync();
        }

        private async Task LoadPermissionAsync()
        {
            try
            {
                _canCreateStore = await _authService.HasPermissionAsync("stores.create");
            }
            catch
            {
                _canCreateStore = false;
            }

            SaveButton.IsVisible = _canCreateStore;
            SaveButton.IsEnabled = _canCreateStore && !_isBusy;

            if (!_canCreateStore)
                ShowMessage("ليس لديك صلاحية لإضافة متجر.");
            else
                MessageLabel.IsVisible = false;
        }

        private async Task LoadStoreTypesAsync()
        {
            try
            {
                _storeTypes = await _storesService.GetStoreTypesAsync(true);

                StoreTypePicker.ItemsSource = _storeTypes;

                if (_storeTypes.Count > 0)
                    StoreTypePicker.SelectedIndex = 0;
            }
            catch
            {
                _storeTypes = new List<StoreTypeDto>();
                StoreTypePicker.ItemsSource = null;
            }
        }

        private void OnNameChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text))
                return;

            var slug = NameEntry.Text
                .Trim()
                .ToLower()
                .Replace(" ", "-");

            SlugEntry.Text = slug;
        }

        private void OnIsActiveToggled(object sender, ToggledEventArgs e)
        {
            IsActiveTextLabel.Text = e.Value ? "المتجر نشط" : "المتجر متوقف";
        }

        private async void OnPickImageClicked(object sender, EventArgs e)
        {
            if (!_canCreateStore || _isBusy)
                return;

            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "اختر شعار المتجر",
                    FileTypes = FilePickerFileType.Images
                });

                if (result == null)
                    return;

                _selectedImagePath = result.FullPath;

                SelectedImageNameLabel.Text = result.FileName;

                LogoPreviewImage.Source = ImageSource.FromFile(result.FullPath);
                LogoPreviewImage.IsVisible = true;

                NoImageLabel.IsVisible = false;

                LogoUrlEntry.Text = result.FullPath;
            }
            catch (Exception ex)
            {
                await DisplayAlert("خطأ", ex.Message, "موافق");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_isBusy || !_canCreateStore)
                return;

            MessageLabel.IsVisible = false;

            var name = NameEntry.Text?.Trim() ?? "";
            var slug = SlugEntry.Text?.Trim() ?? "";
            var selectedStoreType = StoreTypePicker.SelectedItem as StoreTypeDto;

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowMessage("يرجى إدخال اسم المتجر");
                return;
            }

            if (string.IsNullOrWhiteSpace(slug))
            {
                ShowMessage("يرجى إدخال Slug المتجر");
                return;
            }

            if (selectedStoreType == null)
            {
                ShowMessage("يرجى اختيار نوع المتجر");
                return;
            }

            try
            {
                SetBusy(true);

                string? logoUrl = null;

                if (!string.IsNullOrWhiteSpace(_selectedImagePath))
                {
                    logoUrl = await _storesService.UploadLogoAsync(_selectedImagePath, slug, null);

                    if (string.IsNullOrWhiteSpace(logoUrl))
                    {
                        ShowMessage("فشل رفع شعار المتجر.");
                        return;
                    }
                }
                else
                {
                    logoUrl = LogoUrlEntry.Text?.Trim();
                }

                var request = new CreateStoreRequest
                {
                    StoreTypeId = selectedStoreType.Id,
                    Name = name,
                    Slug = slug,
                    Description = DescriptionEditor.Text,
                    Phone = PhoneEntry.Text,
                    Email = EmailEntry.Text,
                    LogoPath = logoUrl,
                    IsActive = IsActiveSwitch.IsToggled
                };

                var result = await _storesService.CreateStoreAsync(request);

                if (!result.IsSuccess)
                {
                    ShowMessage(result.Message);
                    return;
                }

                await DisplayAlert("نجاح", "تم إنشاء المتجر بنجاح", "موافق");

                await Shell.Current.GoToAsync(AppShell.RouteStoresList);
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
            await Shell.Current.GoToAsync(AppShell.RouteStoresList);
        }

        private void SetBusy(bool value)
        {
            _isBusy = value;

            LoadingIndicator.IsVisible = value;
            LoadingIndicator.IsRunning = value;

            SaveButton.IsEnabled = _canCreateStore && !value;
            PickImageButton.IsEnabled = _canCreateStore && !value;
            StoreTypePicker.IsEnabled = _canCreateStore && !value;
        }

        private void ShowMessage(string msg)
        {
            MessageLabel.Text = msg;
            MessageLabel.IsVisible = true;
        }
    }
}