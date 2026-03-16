using Microsoft.Maui.Storage;
using MPStore.Admin.Helpers;
using MPStore.Admin.Models.Stores;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views
{
    [QueryProperty(nameof(StoreId), "storeId")]
    public partial class EditStore : ContentPage
    {
        private readonly StoresService _storesService;
        private bool _isBusy;
        private long _storeId;
        private StoreDto? _currentStore;
        private string? _selectedImagePath;
        private List<StoreTypeDto> _storeTypes = new();

        public string? StoreId
        {
            get => _storeId.ToString();
            set
            {
                if (long.TryParse(value, out var id))
                    _storeId = id;
            }
        }

        public EditStore(StoresService storesService)
        {
            InitializeComponent();
            _storesService = storesService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_storeId <= 0)
                return;

            if (_storeTypes.Count == 0)
                await LoadStoreTypesAsync();

            if (_currentStore == null)
                await LoadStoreAsync();
        }

        private async Task LoadStoreTypesAsync()
        {
            try
            {
                _storeTypes = await _storesService.GetStoreTypesAsync(true);
                StoreTypePicker.ItemsSource = _storeTypes;
            }
            catch
            {
                _storeTypes = new List<StoreTypeDto>();
                StoreTypePicker.ItemsSource = null;
            }
        }

        private async Task LoadStoreAsync()
        {
            if (_isBusy || _storeId <= 0)
                return;

            try
            {
                SetBusy(true);
                HideMessage();

                var store = await _storesService.GetStoreAsync(_storeId);

                if (store == null)
                {
                    ShowMessage("تعذر تحميل بيانات المتجر.");
                    return;
                }

                _currentStore = store;

                NameEntry.Text = store.Name ?? "";
                SlugEntry.Text = store.Slug ?? "";
                DescriptionEditor.Text = store.Description ?? "";

                PhoneEntry.Text = "";
                EmailEntry.Text = "";

                LogoUrlEntry.Text = store.LogoPath ?? "";
                IsActiveSwitch.IsToggled = store.IsActive;
                IsActiveTextLabel.Text = store.IsActive ? "المتجر نشط" : "المتجر متوقف";

                if (_storeTypes.Count > 0)
                {
                    var selectedType = _storeTypes.FirstOrDefault(x => x.Id == store.StoreTypeId);
                    if (selectedType != null)
                        StoreTypePicker.SelectedItem = selectedType;
                }

                if (!string.IsNullOrWhiteSpace(store.LogoPath))
                {
                    var url = ApiConfig.BaseUrl.TrimEnd('/') + store.LogoPath;

                    LogoPreviewImage.Source = ImageSource.FromUri(new Uri(url));
                    LogoPreviewImage.IsVisible = true;
                    NoImageLabel.IsVisible = false;
                    SelectedImageNameLabel.Text = "الشعار الحالي";
                }
                else
                {
                    LogoPreviewImage.Source = null;
                    LogoPreviewImage.IsVisible = false;
                    NoImageLabel.IsVisible = true;
                    SelectedImageNameLabel.Text = "لم يتم اختيار صورة بعد";
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"خطأ أثناء تحميل المتجر: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void OnNameChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text))
                return;

            var slug = GenerateSlug(NameEntry.Text);

            if (string.IsNullOrWhiteSpace(SlugEntry.Text))
                SlugEntry.Text = slug;
        }

        private void OnIsActiveToggled(object sender, ToggledEventArgs e)
        {
            IsActiveTextLabel.Text = e.Value ? "المتجر نشط" : "المتجر متوقف";
        }

        private async void OnPickImageClicked(object sender, EventArgs e)
        {
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
                ShowMessage($"خطأ أثناء اختيار الصورة: {ex.Message}");
            }
        }

        private async void OnOpenDetailsClicked(object sender, EventArgs e)
        {
            if (_storeId <= 0)
                return;

            await Shell.Current.GoToAsync($"{AppShell.RouteAddStoreDetails}?storeId={_storeId}");
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_isBusy)
                return;

            HideMessage();

            var name = NameEntry.Text?.Trim() ?? "";
            var slug = SlugEntry.Text?.Trim() ?? "";
            var selectedStoreType = StoreTypePicker.SelectedItem as StoreTypeDto;

            if (_storeId <= 0)
            {
                ShowMessage("رقم المتجر غير صالح.");
                return;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowMessage("يرجى إدخال اسم المتجر.");
                return;
            }

            if (string.IsNullOrWhiteSpace(slug))
            {
                ShowMessage("يرجى إدخال Slug المتجر.");
                return;
            }

            if (selectedStoreType == null)
            {
                ShowMessage("يرجى اختيار نوع المتجر.");
                return;
            }

            try
            {
                SetBusy(true);

                string? logoPath = _currentStore?.LogoPath;

                if (!string.IsNullOrWhiteSpace(_selectedImagePath))
                {
                    logoPath = await _storesService.UploadLogoAsync(_selectedImagePath, slug, _storeId);

                    if (string.IsNullOrWhiteSpace(logoPath))
                    {
                        ShowMessage("فشل رفع شعار المتجر.");
                        return;
                    }
                }

                var request = new UpdateStoreRequest
                {
                    StoreId = _storeId,
                    StoreTypeId = selectedStoreType.Id,
                    Name = name,
                    Slug = slug,
                    Description = DescriptionEditor.Text?.Trim(),
                    LogoPath = logoPath,
                    IsActive = IsActiveSwitch.IsToggled
                };

                var result = await _storesService.UpdateStoreAsync(request);

                if (!result.IsSuccess)
                {
                    ShowMessage(result.Message);
                    return;
                }

                await DisplayAlert("نجاح", "تم تحديث بيانات المتجر بنجاح.", "موافق");

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

            SaveButton.IsEnabled = !value;
            NameEntry.IsEnabled = !value;
            SlugEntry.IsEnabled = !value;
            DescriptionEditor.IsEnabled = !value;
            PhoneEntry.IsEnabled = !value;
            EmailEntry.IsEnabled = !value;
            LogoUrlEntry.IsEnabled = !value;
            PickImageButton.IsEnabled = !value;
            IsActiveSwitch.IsEnabled = !value;
            StoreTypePicker.IsEnabled = !value;
        }

        private void ShowMessage(string message)
        {
            MessageLabel.Text = message;
            MessageLabel.IsVisible = true;
        }

        private void HideMessage()
        {
            MessageLabel.Text = "";
            MessageLabel.IsVisible = false;
        }

        private async void OnManageUsersClicked(object sender, EventArgs e)
        {
            if (_storeId <= 0)
                return;

            await Shell.Current.GoToAsync($"{AppShell.RouteStoreUsersList}?storeId={_storeId}");
        }

        private static string GenerateSlug(string text)
        {
            var value = text.Trim().ToLowerInvariant();

            value = string.Join("-", value
                .Split(new[] { ' ', '_', '/', '\\', '.', ',', ';', ':', '|', '+' }, StringSplitOptions.RemoveEmptyEntries));

            while (value.Contains("--"))
                value = value.Replace("--", "-");

            return value.Trim('-');
        }
    }
}