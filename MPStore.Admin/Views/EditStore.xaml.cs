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
                LogoUrlEntry.Text = store.LogoPath ?? string.Empty;
                IsActiveSwitch.IsToggled = store.IsActive;
                IsActiveTextLabel.Text = store.IsActive ? "المتجر نشط" : "المتجر متوقف";
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

                if (!result)
                {
                    ShowMessage("فشل حفظ التعديلات.");
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
            await Navigation.PopAsync();
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
            LogoUrlEntry.IsEnabled = !value;
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