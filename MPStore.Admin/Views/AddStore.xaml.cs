using MPStore.Admin.Models.Stores;
using MPStore.Admin.Services;
using Microsoft.Maui.Storage;

namespace MPStore.Admin.Views
{
    public partial class AddStore : ContentPage
    {
        private readonly StoresService _storesService;
        private bool _isBusy;

        private string? _selectedImagePath;

        public AddStore(StoresService storesService)
        {
            InitializeComponent();
            _storesService = storesService;
        }

        private void OnNameChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text))
                return;

            var slug = NameEntry.Text
                .ToLower()
                .Replace(" ", "-")
                .Replace("أ", "a")
                .Replace("إ", "a")
                .Replace("آ", "a")
                .Replace("ة", "h")
                .Replace("ى", "a")
                .Replace("ي", "y");

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
                await DisplayAlert("خطأ", ex.Message, "موافق");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_isBusy)
                return;

            MessageLabel.IsVisible = false;

            var name = NameEntry.Text?.Trim() ?? "";
            var slug = SlugEntry.Text?.Trim() ?? "";

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

            SaveButton.IsEnabled = !value;
        }

        private void ShowMessage(string msg)
        {
            MessageLabel.Text = msg;
            MessageLabel.IsVisible = true;
        }
    }
}