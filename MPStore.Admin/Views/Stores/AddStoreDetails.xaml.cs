using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Storage;
using MPStore.Admin.Helpers;
using MPStore.Admin.Models.Stores;
using MPStore.Admin.Services;
using System.Globalization;
using System.Text.Json;

namespace MPStore.Admin.Views
{
    [QueryProperty(nameof(StoreId), "storeId")]
    public partial class AddStoreDetails : ContentPage
    {
        public const string DraftPreferenceKey = "AddStoreDetailsDraft";

        private readonly StoresService _storesService;

        private bool _isBusy;
        private bool _isEditLoaded;
        private string? _selectedCoverImagePath;
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

        public AddStoreDetails(StoresService storesService)
        {
            InitializeComponent();
            _storesService = storesService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            UpdateSwitchTexts();

            if (_storeId > 0)
            {
                if (!_isEditLoaded)
                    await LoadEditStoreAsync();
            }
            else
            {
                LoadDraft();
            }
        }

        private async Task LoadEditStoreAsync()
        {
            if (_isBusy || _storeId <= 0)
                return;

            try
            {
                SetBusy(true);
                MessageLabel.IsVisible = false;

                var store = await _storesService.GetStoreAsync(_storeId);
                if (store == null)
                {
                    ShowMessage("تعذر تحميل تفاصيل المتجر.");
                    return;
                }

                _currentStore = store;
                _isEditLoaded = true;

                OwnerNameEntry.Text = store.OwnerName ?? string.Empty;
                LegalNameEntry.Text = store.LegalName ?? string.Empty;
                ShortDescriptionEditor.Text = store.ShortDescription ?? string.Empty;
                WhatsAppNumberEntry.Text = store.WhatsAppNumber ?? string.Empty;
                WebsiteUrlEntry.Text = store.WebsiteUrl ?? string.Empty;
                CountryEntry.Text = store.Country ?? string.Empty;
                CityEntry.Text = store.City ?? string.Empty;
                RegionEntry.Text = store.Region ?? string.Empty;
                AddressLine1Entry.Text = store.AddressLine1 ?? string.Empty;
                AddressLine2Entry.Text = store.AddressLine2 ?? string.Empty;
                PostalCodeEntry.Text = store.PostalCode ?? string.Empty;
                LatitudeEntry.Text = store.Latitude?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                LongitudeEntry.Text = store.Longitude?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                CoverImagePathEntry.Text = store.CoverImagePath ?? string.Empty;
                TaxNumberEntry.Text = store.TaxNumber ?? string.Empty;
                CommercialRegistrationNoEntry.Text = store.CommercialRegistrationNo ?? string.Empty;
                WorkingHoursJsonEditor.Text = store.WorkingHoursJson ?? string.Empty;
                FacebookUrlEntry.Text = store.FacebookUrl ?? string.Empty;
                InstagramUrlEntry.Text = store.InstagramUrl ?? string.Empty;
                TelegramUrlEntry.Text = store.TelegramUrl ?? string.Empty;
                XUrlEntry.Text = store.XUrl ?? string.Empty;
                IsVerifiedSwitch.IsToggled = store.IsVerified;
                IsFeaturedSwitch.IsToggled = store.IsFeatured;

                if (!string.IsNullOrWhiteSpace(store.CoverImagePath))
                {
                    var url = ApiConfig.BaseUrl.TrimEnd('/') + store.CoverImagePath;
                    CoverPreviewImage.Source = ImageSource.FromUri(new Uri(url));
                    CoverPreviewImage.IsVisible = true;
                    NoCoverImageLabel.IsVisible = false;
                    SelectedCoverImageNameLabel.Text = "الغلاف الحالي";
                }
                else
                {
                    _selectedCoverImagePath = null;
                    SelectedCoverImageNameLabel.Text = "لم يتم اختيار صورة غلاف بعد";
                    CoverPreviewImage.Source = null;
                    CoverPreviewImage.IsVisible = false;
                    NoCoverImageLabel.IsVisible = true;
                }

                LocationStatusLabel.Text = "يمكنك جلب الإحداثيات من GPS أو تعديلها يدويًا";
                UpdateSwitchTexts();
            }
            catch (Exception ex)
            {
                ShowMessage($"خطأ أثناء تحميل التفاصيل: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void OnIsVerifiedToggled(object sender, ToggledEventArgs e)
        {
            IsVerifiedTextLabel.Text = e.Value ? "المتجر موثق" : "المتجر غير موثق";
        }

        private void OnIsFeaturedToggled(object sender, ToggledEventArgs e)
        {
            IsFeaturedTextLabel.Text = e.Value ? "المتجر مميز" : "المتجر غير مميز";
        }

        private async void OnGetCurrentLocationClicked(object sender, EventArgs e)
        {
            if (_isBusy)
                return;

            try
            {
                LocationStatusLabel.Text = "جارٍ طلب صلاحية الموقع...";
                var permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                if (permission != PermissionStatus.Granted)
                {
                    LocationStatusLabel.Text = "تم رفض صلاحية الموقع.";
                    return;
                }

                SetBusy(true);
                LocationStatusLabel.Text = "جارٍ جلب الموقع الحالي...";

                Location? location = null;

                try
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(15));
                    location = await Geolocation.Default.GetLocationAsync(request);
                }
                catch
                {
                }

                if (location == null)
                {
                    try
                    {
                        location = await Geolocation.Default.GetLastKnownLocationAsync();
                    }
                    catch
                    {
                    }
                }

                if (location == null)
                {
                    LocationStatusLabel.Text = "تعذر الحصول على الموقع الحالي.";
                    return;
                }

                LatitudeEntry.Text = location.Latitude.ToString(CultureInfo.InvariantCulture);
                LongitudeEntry.Text = location.Longitude.ToString(CultureInfo.InvariantCulture);
                LocationStatusLabel.Text = "تم جلب الموقع الحالي بنجاح.";
            }
            catch (FeatureNotSupportedException)
            {
                LocationStatusLabel.Text = "خدمة الموقع غير مدعومة على هذا الجهاز.";
            }
            catch (FeatureNotEnabledException)
            {
                LocationStatusLabel.Text = "خدمة الموقع غير مفعلة على الجهاز.";
            }
            catch (PermissionException)
            {
                LocationStatusLabel.Text = "لا توجد صلاحية للوصول إلى الموقع.";
            }
            catch (Exception ex)
            {
                LocationStatusLabel.Text = $"خطأ أثناء جلب الموقع: {ex.Message}";
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void OnPickCoverImageClicked(object sender, EventArgs e)
        {
            if (_isBusy)
                return;

            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "اختر صورة غلاف المتجر",
                    FileTypes = FilePickerFileType.Images
                });

                if (result == null)
                    return;

                _selectedCoverImagePath = result.FullPath;
                SelectedCoverImageNameLabel.Text = result.FileName;
                CoverImagePathEntry.Text = result.FullPath;

                CoverPreviewImage.Source = ImageSource.FromFile(result.FullPath);
                CoverPreviewImage.IsVisible = true;
                NoCoverImageLabel.IsVisible = false;
            }
            catch (Exception ex)
            {
                ShowMessage($"خطأ: {ex.Message}");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_isBusy)
                return;

            MessageLabel.IsVisible = false;

            if (!TryParseDecimalOrEmpty(LatitudeEntry.Text, out var latitude))
            {
                ShowMessage("قيمة خط العرض غير صحيحة.");
                return;
            }

            if (!TryParseDecimalOrEmpty(LongitudeEntry.Text, out var longitude))
            {
                ShowMessage("قيمة خط الطول غير صحيحة.");
                return;
            }

            try
            {
                SetBusy(true);

                string? coverImagePath = CoverImagePathEntry.Text?.Trim();

                if (!string.IsNullOrWhiteSpace(_selectedCoverImagePath))
                {
                    if (_storeId > 0)
                        coverImagePath = await _storesService.UploadCoverAsync(_selectedCoverImagePath, _currentStore?.Slug, _storeId);
                    else
                        coverImagePath = await _storesService.UploadCoverAsync(_selectedCoverImagePath);

                    if (string.IsNullOrWhiteSpace(coverImagePath))
                    {
                        ShowMessage("فشل رفع صورة الغلاف.");
                        return;
                    }
                }

                if (_storeId > 0)
                {
                    if (_currentStore == null)
                    {
                        ShowMessage("بيانات المتجر الحالية غير متوفرة.");
                        return;
                    }

                    var request = new UpdateStoreRequest
                    {
                        StoreId = _currentStore.Id,
                        StoreTypeId = _currentStore.StoreTypeId,
                        Name = _currentStore.Name,
                        Slug = _currentStore.Slug,
                        Description = Normalize(_currentStore.Description),
                        Phone = Normalize(_currentStore.Phone),
                        Email = Normalize(_currentStore.Email),
                        LogoPath = Normalize(_currentStore.LogoPath),
                        IsActive = _currentStore.IsActive,

                        OwnerName = Normalize(OwnerNameEntry.Text),
                        LegalName = Normalize(LegalNameEntry.Text),
                        ShortDescription = Normalize(ShortDescriptionEditor.Text),
                        WhatsAppNumber = Normalize(WhatsAppNumberEntry.Text),
                        WebsiteUrl = Normalize(WebsiteUrlEntry.Text),
                        Country = Normalize(CountryEntry.Text),
                        City = Normalize(CityEntry.Text),
                        Region = Normalize(RegionEntry.Text),
                        AddressLine1 = Normalize(AddressLine1Entry.Text),
                        AddressLine2 = Normalize(AddressLine2Entry.Text),
                        PostalCode = Normalize(PostalCodeEntry.Text),
                        Latitude = latitude,
                        Longitude = longitude,
                        CoverImagePath = Normalize(coverImagePath),
                        TaxNumber = Normalize(TaxNumberEntry.Text),
                        CommercialRegistrationNo = Normalize(CommercialRegistrationNoEntry.Text),
                        WorkingHoursJson = Normalize(WorkingHoursJsonEditor.Text),
                        FacebookUrl = Normalize(FacebookUrlEntry.Text),
                        InstagramUrl = Normalize(InstagramUrlEntry.Text),
                        TelegramUrl = Normalize(TelegramUrlEntry.Text),
                        XUrl = Normalize(XUrlEntry.Text),
                        IsVerified = IsVerifiedSwitch.IsToggled,
                        IsFeatured = IsFeaturedSwitch.IsToggled
                    };

                    var result = await _storesService.UpdateStoreAsync(request);

                    if (!result.IsSuccess)
                    {
                        ShowMessage(result.Message);
                        return;
                    }

                    await DisplayAlert("نجاح", "تم حفظ التفاصيل بنجاح.", "موافق");
                    await Shell.Current.GoToAsync($"{AppShell.RouteEditStore}?storeId={_storeId}");
                    return;
                }

                var draft = new AddStoreDetailsDraft
                {
                    StoreId = 0,
                    OwnerName = Normalize(OwnerNameEntry.Text),
                    LegalName = Normalize(LegalNameEntry.Text),
                    ShortDescription = Normalize(ShortDescriptionEditor.Text),
                    WhatsAppNumber = Normalize(WhatsAppNumberEntry.Text),
                    WebsiteUrl = Normalize(WebsiteUrlEntry.Text),
                    Country = Normalize(CountryEntry.Text),
                    City = Normalize(CityEntry.Text),
                    Region = Normalize(RegionEntry.Text),
                    AddressLine1 = Normalize(AddressLine1Entry.Text),
                    AddressLine2 = Normalize(AddressLine2Entry.Text),
                    PostalCode = Normalize(PostalCodeEntry.Text),
                    Latitude = latitude,
                    Longitude = longitude,
                    CoverImagePath = Normalize(coverImagePath),
                    TaxNumber = Normalize(TaxNumberEntry.Text),
                    CommercialRegistrationNo = Normalize(CommercialRegistrationNoEntry.Text),
                    WorkingHoursJson = Normalize(WorkingHoursJsonEditor.Text),
                    FacebookUrl = Normalize(FacebookUrlEntry.Text),
                    InstagramUrl = Normalize(InstagramUrlEntry.Text),
                    TelegramUrl = Normalize(TelegramUrlEntry.Text),
                    XUrl = Normalize(XUrlEntry.Text),
                    IsVerified = IsVerifiedSwitch.IsToggled,
                    IsFeatured = IsFeaturedSwitch.IsToggled
                };

                Preferences.Set(DraftPreferenceKey, JsonSerializer.Serialize(draft));
                await Shell.Current.GoToAsync(AppShell.RouteAddStore);
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
            if (_storeId > 0)
                await Shell.Current.GoToAsync($"{AppShell.RouteEditStore}?storeId={_storeId}");
            else
                await Shell.Current.GoToAsync(AppShell.RouteAddStore);
        }

        private void LoadDraft()
        {
            try
            {
                var json = Preferences.Get(DraftPreferenceKey, string.Empty);
                if (string.IsNullOrWhiteSpace(json))
                {
                    ClearForm();
                    return;
                }

                var draft = JsonSerializer.Deserialize<AddStoreDetailsDraft>(json);
                if (draft == null || draft.StoreId > 0)
                {
                    ClearForm();
                    return;
                }

                OwnerNameEntry.Text = draft.OwnerName;
                LegalNameEntry.Text = draft.LegalName;
                ShortDescriptionEditor.Text = draft.ShortDescription;
                WhatsAppNumberEntry.Text = draft.WhatsAppNumber;
                WebsiteUrlEntry.Text = draft.WebsiteUrl;
                CountryEntry.Text = draft.Country;
                CityEntry.Text = draft.City;
                RegionEntry.Text = draft.Region;
                AddressLine1Entry.Text = draft.AddressLine1;
                AddressLine2Entry.Text = draft.AddressLine2;
                PostalCodeEntry.Text = draft.PostalCode;
                LatitudeEntry.Text = draft.Latitude?.ToString(CultureInfo.InvariantCulture);
                LongitudeEntry.Text = draft.Longitude?.ToString(CultureInfo.InvariantCulture);
                CoverImagePathEntry.Text = draft.CoverImagePath;
                TaxNumberEntry.Text = draft.TaxNumber;
                CommercialRegistrationNoEntry.Text = draft.CommercialRegistrationNo;
                WorkingHoursJsonEditor.Text = draft.WorkingHoursJson;
                FacebookUrlEntry.Text = draft.FacebookUrl;
                InstagramUrlEntry.Text = draft.InstagramUrl;
                TelegramUrlEntry.Text = draft.TelegramUrl;
                XUrlEntry.Text = draft.XUrl;
                IsVerifiedSwitch.IsToggled = draft.IsVerified;
                IsFeaturedSwitch.IsToggled = draft.IsFeatured;

                if (!string.IsNullOrWhiteSpace(draft.CoverImagePath))
                {
                    if (File.Exists(draft.CoverImagePath))
                    {
                        _selectedCoverImagePath = draft.CoverImagePath;
                        SelectedCoverImageNameLabel.Text = Path.GetFileName(draft.CoverImagePath);
                        CoverPreviewImage.Source = ImageSource.FromFile(draft.CoverImagePath);
                        CoverPreviewImage.IsVisible = true;
                        NoCoverImageLabel.IsVisible = false;
                    }
                    else
                    {
                        _selectedCoverImagePath = null;
                        var url = draft.CoverImagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                            ? draft.CoverImagePath
                            : ApiConfig.BaseUrl.TrimEnd('/') + draft.CoverImagePath;

                        CoverPreviewImage.Source = ImageSource.FromUri(new Uri(url));
                        CoverPreviewImage.IsVisible = true;
                        NoCoverImageLabel.IsVisible = false;
                        SelectedCoverImageNameLabel.Text = "الغلاف الحالي";
                    }
                }
                else
                {
                    _selectedCoverImagePath = null;
                    SelectedCoverImageNameLabel.Text = "لم يتم اختيار صورة غلاف بعد";
                    CoverPreviewImage.IsVisible = false;
                    NoCoverImageLabel.IsVisible = true;
                }

                LocationStatusLabel.Text = "يمكنك جلب الإحداثيات من GPS أو تعديلها يدويًا";
            }
            catch
            {
                ClearForm();
            }
        }

        private void ClearForm()
        {
            OwnerNameEntry.Text = string.Empty;
            LegalNameEntry.Text = string.Empty;
            ShortDescriptionEditor.Text = string.Empty;
            WhatsAppNumberEntry.Text = string.Empty;
            WebsiteUrlEntry.Text = string.Empty;
            CountryEntry.Text = string.Empty;
            CityEntry.Text = string.Empty;
            RegionEntry.Text = string.Empty;
            AddressLine1Entry.Text = string.Empty;
            AddressLine2Entry.Text = string.Empty;
            PostalCodeEntry.Text = string.Empty;
            LatitudeEntry.Text = string.Empty;
            LongitudeEntry.Text = string.Empty;
            CoverImagePathEntry.Text = string.Empty;
            TaxNumberEntry.Text = string.Empty;
            CommercialRegistrationNoEntry.Text = string.Empty;
            WorkingHoursJsonEditor.Text = string.Empty;
            FacebookUrlEntry.Text = string.Empty;
            InstagramUrlEntry.Text = string.Empty;
            TelegramUrlEntry.Text = string.Empty;
            XUrlEntry.Text = string.Empty;
            IsVerifiedSwitch.IsToggled = false;
            IsFeaturedSwitch.IsToggled = false;
            _selectedCoverImagePath = null;
            SelectedCoverImageNameLabel.Text = "لم يتم اختيار صورة غلاف بعد";
            CoverPreviewImage.Source = null;
            CoverPreviewImage.IsVisible = false;
            NoCoverImageLabel.IsVisible = true;
            LocationStatusLabel.Text = "يمكنك جلب الإحداثيات من GPS أو تعديلها يدويًا";
        }

        private void UpdateSwitchTexts()
        {
            IsVerifiedTextLabel.Text = IsVerifiedSwitch.IsToggled ? "المتجر موثق" : "المتجر غير موثق";
            IsFeaturedTextLabel.Text = IsFeaturedSwitch.IsToggled ? "المتجر مميز" : "المتجر غير مميز";
        }

        private void SetBusy(bool value)
        {
            _isBusy = value;

            LoadingIndicator.IsVisible = value;
            LoadingIndicator.IsRunning = value;

            SaveButton.IsEnabled = !value;
            PickCoverImageButton.IsEnabled = !value;
            GetCurrentLocationButton.IsEnabled = !value;
        }

        private void ShowMessage(string msg)
        {
            MessageLabel.Text = msg;
            MessageLabel.IsVisible = true;
        }

        private static string? Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Trim();
        }

        private static bool TryParseDecimalOrEmpty(string? text, out decimal? value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(text))
                return true;

            var raw = text.Trim().Replace("،", ".").Replace(",", ".");

            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                value = parsed;
                return true;
            }

            return false;
        }

        public static AddStoreDetailsDraft? GetSavedDraft()
        {
            try
            {
                var json = Preferences.Get(DraftPreferenceKey, string.Empty);
                if (string.IsNullOrWhiteSpace(json))
                    return null;

                return JsonSerializer.Deserialize<AddStoreDetailsDraft>(json);
            }
            catch
            {
                return null;
            }
        }

        public static void ClearSavedDraft()
        {
            Preferences.Remove(DraftPreferenceKey);
        }

        public sealed class AddStoreDetailsDraft
        {
            public long StoreId { get; set; }
            public string? OwnerName { get; set; }
            public string? LegalName { get; set; }
            public string? ShortDescription { get; set; }
            public string? WhatsAppNumber { get; set; }
            public string? WebsiteUrl { get; set; }
            public string? Country { get; set; }
            public string? City { get; set; }
            public string? Region { get; set; }
            public string? AddressLine1 { get; set; }
            public string? AddressLine2 { get; set; }
            public string? PostalCode { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public string? CoverImagePath { get; set; }
            public string? TaxNumber { get; set; }
            public string? CommercialRegistrationNo { get; set; }
            public string? WorkingHoursJson { get; set; }
            public string? FacebookUrl { get; set; }
            public string? InstagramUrl { get; set; }
            public string? TelegramUrl { get; set; }
            public string? XUrl { get; set; }
            public bool IsVerified { get; set; }
            public bool IsFeatured { get; set; }
        }
    }
}