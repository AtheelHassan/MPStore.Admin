using MPStore.Admin.Models.Stores;
using MPStore.Admin.Services;
using Microsoft.Maui.Storage;

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

            if (_currentStore == null && _storeId > 0)
                await LoadStoreAsync();
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

                NameEntry.Text = store.Name ?? string.Empty;
                SlugEntry.Text = store.Slug ?? string.Empty;
                DescriptionEditor.Text = store.Description ?? string.Empty;

                PhoneEntry.Text = string.Empty;
                EmailEntry.Text = string.Empty;

                LogoUrlEntry.Text = store.LogoPath ?? string.Empty;
                IsActiveSwitch.IsToggled = store.IsActive;
                IsActiveTextLabel.Text = store.IsActive ? "المتجر نشط" : "المتجر متوقف";

                if (!string.IsNullOrWhiteSpace(store.LogoPath))
                {
                    LogoPreviewImage.Source = ImageSource.FromUri(new Uri(store.LogoPath));
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

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_isBusy)
                return;

            HideMessage();

            var name = NameEntry.Text?.Trim() ?? string.Empty;
            var slug = SlugEntry.Text?.Trim() ?? string.Empty;

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

            try
            {
                SetBusy(true);

                var request = new UpdateStoreRequest
                {
                    StoreId = _storeId,
                    Name = name,
                    Slug = slug,
                    Description = DescriptionEditor.Text?.Trim(),
                    LogoPath = LogoUrlEntry.Text?.Trim(),
                    IsActive = IsActiveSwitch.IsToggled
                };

                var result = await _storesService.UpdateStoreAsync(request);

                if (!result.IsSuccess)
                {
                    ShowMessage(result.Message);
                    return;
                }

                await DisplayAlert("نجاح", "تم تحديث بيانات المتجر بنجاح.", "موافق");
                await Navigation.PopAsync();
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
            await Shell.Current.GoToAsync($"//{nameof(StoresList)}");
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
        }

        private void ShowMessage(string message)
        {
            MessageLabel.Text = message;
            MessageLabel.IsVisible = true;
        }

        private void HideMessage()
        {
            MessageLabel.Text = string.Empty;
            MessageLabel.IsVisible = false;
        }


        private async void OnManageUsersClicked(object sender, EventArgs e)
        {
            if (_storeId <= 0)
                return;

            await Shell.Current.GoToAsync($"StoreUsersList?storeId={_storeId}");
        }
        private static string GenerateSlug(string text)
        {
            var value = text.Trim().ToLowerInvariant();

            value = value
                .Replace("أ", "a")
                .Replace("إ", "a")
                .Replace("آ", "a")
                .Replace("ا", "a")
                .Replace("ب", "b")
                .Replace("ت", "t")
                .Replace("ث", "th")
                .Replace("ج", "j")
                .Replace("ح", "h")
                .Replace("خ", "kh")
                .Replace("د", "d")
                .Replace("ذ", "th")
                .Replace("ر", "r")
                .Replace("ز", "z")
                .Replace("س", "s")
                .Replace("ش", "sh")
                .Replace("ص", "s")
                .Replace("ض", "d")
                .Replace("ط", "t")
                .Replace("ظ", "z")
                .Replace("ع", "a")
                .Replace("غ", "gh")
                .Replace("ف", "f")
                .Replace("ق", "q")
                .Replace("ك", "k")
                .Replace("ل", "l")
                .Replace("م", "m")
                .Replace("ن", "n")
                .Replace("ه", "h")
                .Replace("ة", "h")
                .Replace("و", "w")
                .Replace("ي", "y")
                .Replace("ى", "a")
                .Replace("ء", "")
                .Replace("ؤ", "w")
                .Replace("ئ", "y");

            value = string.Join("-", value
                .Split(new[] { ' ', '_', '/', '\\', '.', ',', ';', ':', '|', '+' }, StringSplitOptions.RemoveEmptyEntries));

            while (value.Contains("--"))
                value = value.Replace("--", "-");

            return value.Trim('-');
        }
    }
}