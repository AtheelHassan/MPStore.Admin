using System.Text.Json;
using MPStore.Admin.Models.Stores;
using MPStore.Admin.Services;
using Microsoft.Maui.Storage;

namespace MPStore.Admin.Views
{
    public partial class AddStore : ContentPage
    {
        private const string DraftPreferenceKey = "AddStoreBasicDraft";

        private readonly StoresService _storesService;
        private readonly AuthService _authService;
        private bool _isBusy;
        private bool _canCreateStore;
        private bool _isReturningFromDetails;

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

            if (_isReturningFromDetails)
            {
                LoadDraftIfAny();
                _isReturningFromDetails = false;
            }
            else
            {
                ClearDraft();
                AddStoreDetails.ClearSavedDraft();
                ResetForm();
            }
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

                if (StoreTypePicker.SelectedItem == null && _storeTypes.Count > 0)
                    StoreTypePicker.SelectedIndex = 0;
            }
            catch
            {
                _storeTypes = new List<StoreTypeDto>();
                StoreTypePicker.ItemsSource = null;
            }
        }

        private void ResetForm()
        {
            NameEntry.Text = string.Empty;
            SlugEntry.Text = string.Empty;
            DescriptionEditor.Text = string.Empty;
            PhoneEntry.Text = string.Empty;
            EmailEntry.Text = string.Empty;
            LogoUrlEntry.Text = string.Empty;
            IsActiveSwitch.IsToggled = true;
            IsActiveTextLabel.Text = "المتجر نشط";

            _selectedImagePath = null;
            SelectedImageNameLabel.Text = "لم يتم اختيار صورة بعد";
            LogoPreviewImage.Source = null;
            LogoPreviewImage.IsVisible = false;
            NoImageLabel.IsVisible = true;

            if (_storeTypes.Count > 0)
                StoreTypePicker.SelectedIndex = 0;
        }

        private void LoadDraftIfAny()
        {
            try
            {
                var json = Preferences.Get(DraftPreferenceKey, string.Empty);
                if (string.IsNullOrWhiteSpace(json))
                    return;

                var draft = JsonSerializer.Deserialize<AddStoreBasicDraft>(json);
                if (draft == null)
                    return;

                NameEntry.Text = draft.Name;
                SlugEntry.Text = draft.Slug;
                DescriptionEditor.Text = draft.Description;
                PhoneEntry.Text = draft.Phone;
                EmailEntry.Text = draft.Email;
                LogoUrlEntry.Text = draft.LogoPath;
                IsActiveSwitch.IsToggled = draft.IsActive;
                IsActiveTextLabel.Text = draft.IsActive ? "المتجر نشط" : "المتجر متوقف";

                if (!string.IsNullOrWhiteSpace(draft.LogoLocalFilePath) && File.Exists(draft.LogoLocalFilePath))
                {
                    _selectedImagePath = draft.LogoLocalFilePath;
                    SelectedImageNameLabel.Text = Path.GetFileName(draft.LogoLocalFilePath);
                    LogoPreviewImage.Source = ImageSource.FromFile(draft.LogoLocalFilePath);
                    LogoPreviewImage.IsVisible = true;
                    NoImageLabel.IsVisible = false;
                }
                else
                {
                    _selectedImagePath = null;
                    SelectedImageNameLabel.Text = "لم يتم اختيار صورة بعد";
                    LogoPreviewImage.Source = null;
                    LogoPreviewImage.IsVisible = false;
                    NoImageLabel.IsVisible = true;
                }

                if (_storeTypes.Count > 0)
                {
                    var selectedType = _storeTypes.FirstOrDefault(x => x.Id == draft.StoreTypeId);
                    if (selectedType != null)
                        StoreTypePicker.SelectedItem = selectedType;
                }
            }
            catch
            {
            }
        }

        private void SaveDraft()
        {
            try
            {
                var selectedStoreType = StoreTypePicker.SelectedItem as StoreTypeDto;

                var draft = new AddStoreBasicDraft
                {
                    StoreTypeId = selectedStoreType?.Id ?? 0,
                    Name = NameEntry.Text?.Trim(),
                    Slug = SlugEntry.Text?.Trim(),
                    Description = DescriptionEditor.Text,
                    Phone = PhoneEntry.Text,
                    Email = EmailEntry.Text,
                    LogoPath = LogoUrlEntry.Text?.Trim(),
                    LogoLocalFilePath = _selectedImagePath,
                    IsActive = IsActiveSwitch.IsToggled
                };

                var json = JsonSerializer.Serialize(draft);
                Preferences.Set(DraftPreferenceKey, json);
            }
            catch
            {
            }
        }

        private static void ClearDraft()
        {
            Preferences.Remove(DraftPreferenceKey);
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

                SaveDraft();
            }
            catch (Exception ex)
            {
                await DisplayAlert("خطأ", ex.Message, "موافق");
            }
        }

        private async void OnOpenDetailsClicked(object sender, EventArgs e)
        {
            if (_isBusy || !_canCreateStore)
                return;

            SaveDraft();
            _isReturningFromDetails = true;
            await Shell.Current.GoToAsync(AppShell.RouteAddStoreDetails);
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

                var detailsDraft = AddStoreDetails.GetSavedDraft();

                var request = new CreateStoreRequest
                {
                    StoreTypeId = selectedStoreType.Id,
                    Name = name,
                    Slug = slug,
                    Description = DescriptionEditor.Text,
                    Phone = PhoneEntry.Text,
                    Email = EmailEntry.Text,
                    LogoPath = logoUrl,
                    IsActive = IsActiveSwitch.IsToggled,
                    OwnerName = detailsDraft?.OwnerName,
                    LegalName = detailsDraft?.LegalName,
                    ShortDescription = detailsDraft?.ShortDescription,
                    WhatsAppNumber = detailsDraft?.WhatsAppNumber,
                    WebsiteUrl = detailsDraft?.WebsiteUrl,
                    Country = detailsDraft?.Country,
                    City = detailsDraft?.City,
                    Region = detailsDraft?.Region,
                    AddressLine1 = detailsDraft?.AddressLine1,
                    AddressLine2 = detailsDraft?.AddressLine2,
                    PostalCode = detailsDraft?.PostalCode,
                    Latitude = detailsDraft?.Latitude,
                    Longitude = detailsDraft?.Longitude,
                    CoverImagePath = detailsDraft?.CoverImagePath,
                    TaxNumber = detailsDraft?.TaxNumber,
                    CommercialRegistrationNo = detailsDraft?.CommercialRegistrationNo,
                    WorkingHoursJson = detailsDraft?.WorkingHoursJson,
                    FacebookUrl = detailsDraft?.FacebookUrl,
                    InstagramUrl = detailsDraft?.InstagramUrl,
                    TelegramUrl = detailsDraft?.TelegramUrl,
                    XUrl = detailsDraft?.XUrl,
                    IsVerified = detailsDraft?.IsVerified ?? false,
                    IsFeatured = detailsDraft?.IsFeatured ?? false
                };

                var result = await _storesService.CreateStoreAsync(request);

                if (!result.IsSuccess)
                {
                    ShowMessage(result.Message);
                    return;
                }

                ClearDraft();
                AddStoreDetails.ClearSavedDraft();

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
            ClearDraft();
            AddStoreDetails.ClearSavedDraft();
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
            OpenDetailsButton.IsEnabled = _canCreateStore && !value;
        }

        private void ShowMessage(string msg)
        {
            MessageLabel.Text = msg;
            MessageLabel.IsVisible = true;
        }

        private sealed class AddStoreBasicDraft
        {
            public long StoreTypeId { get; set; }
            public string? Name { get; set; }
            public string? Slug { get; set; }
            public string? Description { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public string? LogoPath { get; set; }
            public string? LogoLocalFilePath { get; set; }
            public bool IsActive { get; set; } = true;
        }
    }
}