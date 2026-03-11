using MPStore.Admin.Models.Stores;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views
{
    public partial class AddStore : ContentPage
    {
        private readonly StoresService _storesService;
        private bool _isBusy;

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

                var request = new CreateStoreRequest
                {
                    Name = name,
                    Slug = slug,
                    Description = DescriptionEditor.Text,
                    Phone = PhoneEntry.Text,
                    Email = EmailEntry.Text,
                    LogoUrl = LogoUrlEntry.Text,
                    IsActive = IsActiveSwitch.IsToggled
                };

                var result = await _storesService.CreateStoreAsync(request);

                if (!result)
                {
                    ShowMessage("فشل حفظ المتجر");
                    return;
                }

                await DisplayAlert("نجاح", "تم إنشاء المتجر بنجاح", "موافق");

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
        }

        private void ShowMessage(string msg)
        {
            MessageLabel.Text = msg;
            MessageLabel.IsVisible = true;
        }
    }
}