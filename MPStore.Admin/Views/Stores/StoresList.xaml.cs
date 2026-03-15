using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MPStore.Admin.Helpers;
using MPStore.Admin.Models.Stores;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views;

public partial class StoresList : ContentPage, INotifyPropertyChanged
{
    private readonly StoresService _storesService;
    private readonly AuthService _authService;
    private bool _isBusy;
    private bool _permissionsLoaded;
    private bool _filtersVisible;

    private readonly List<StoreDto> _allStores = new();
    private List<StoreTypeDto> _storeTypes = new();

    private bool _canCreateStore;
    public bool CanCreateStore
    {
        get => _canCreateStore;
        set
        {
            if (_canCreateStore == value) return;
            _canCreateStore = value;
            OnPropertyChanged();
        }
    }

    private bool _canUpdateStore;
    public bool CanUpdateStore
    {
        get => _canUpdateStore;
        set
        {
            if (_canUpdateStore == value) return;
            _canUpdateStore = value;
            OnPropertyChanged();
        }
    }

    private bool _canDeleteStore;
    public bool CanDeleteStore
    {
        get => _canDeleteStore;
        set
        {
            if (_canDeleteStore == value) return;
            _canDeleteStore = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<StoreListItemViewModel> Stores { get; } = new();

    public ICommand EditCommand { get; }
    public ICommand ViewCommand { get; }
    public ICommand DeleteCommand { get; }

    public new event PropertyChangedEventHandler? PropertyChanged;

    public StoresList(StoresService storesService, AuthService authService)
    {
        InitializeComponent();

        _storesService = storesService;
        _authService = authService;
        BindingContext = this;

        StoresCollectionView.ItemsSource = Stores;

        EditCommand = new Command<StoreListItemViewModel>(async item => await GoToEditAsync(item));
        ViewCommand = new Command<StoreListItemViewModel>(async item => await GoToEditAsync(item));
        DeleteCommand = new Command<StoreListItemViewModel>(async item => await DeleteStoreAsync(item));

        StatusFilterPicker.ItemsSource = new List<string>
        {
            "كل الحالات",
            "نشط فقط",
            "غير نشط فقط"
        };
        StatusFilterPicker.SelectedIndex = 0;

        SortPicker.ItemsSource = new List<string>
        {
            "الأحدث أولاً",
            "الأقدم أولاً",
            "الاسم أ-ي",
            "الاسم ي-أ"
        };
        SortPicker.SelectedIndex = 0;

        SetFiltersVisibility(false);
        UpdateFiltersSummary();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await EnsurePermissionsLoadedAsync();
        await LoadStoreTypesAsync();
        await LoadStoresAsync();
    }

    private async Task EnsurePermissionsLoadedAsync()
    {
        if (_permissionsLoaded)
            return;

        CanCreateStore = await _authService.HasPermissionAsync("stores.create");
        CanUpdateStore = await _authService.HasPermissionAsync("stores.update");
        CanDeleteStore = await _authService.HasPermissionAsync("stores.delete");

        _permissionsLoaded = true;
    }

    private async Task LoadStoreTypesAsync()
    {
        try
        {
            _storeTypes = await _storesService.GetStoreTypesAsync(true);

            var items = new List<StoreTypeFilterItem>
            {
                new() { Id = 0, Name = "كل الأنواع" }
            };

            items.AddRange(_storeTypes.Select(x => new StoreTypeFilterItem
            {
                Id = x.Id,
                Name = x.Name
            }));

            StoreTypeFilterPicker.ItemsSource = items;
            StoreTypeFilterPicker.SelectedIndex = 0;
        }
        catch
        {
            StoreTypeFilterPicker.ItemsSource = new List<StoreTypeFilterItem>
            {
                new() { Id = 0, Name = "كل الأنواع" }
            };
            StoreTypeFilterPicker.SelectedIndex = 0;
        }
    }

    private async Task LoadStoresAsync()
    {
        if (_isBusy)
            return;

        try
        {
            _isBusy = true;
            SetLoading(true);
            ShowMessage(null);

            Stores.Clear();
            _allStores.Clear();

            var result = await _storesService.GetStoresAsync();

            if (result == null || result.Count == 0)
            {
                ApplyFilters();
                return;
            }

            _allStores.AddRange(result);
            ApplyFilters();
        }
        catch (Exception ex)
        {
            ShowMessage($"تعذر تحميل المتاجر: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
            _isBusy = false;
        }
    }

    private void ApplyFilters()
    {
        Stores.Clear();

        IEnumerable<StoreDto> query = _allStores;

        var searchText = SearchEntry.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(x =>
                (!string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(x.Slug) && x.Slug.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(x.Description) && x.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(x.StoreTypeName) && x.StoreTypeName.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(x.StoreTypeCode) && x.StoreTypeCode.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
        }

        var selectedType = StoreTypeFilterPicker.SelectedItem as StoreTypeFilterItem;
        if (selectedType != null && selectedType.Id > 0)
            query = query.Where(x => x.StoreTypeId == selectedType.Id);

        switch (StatusFilterPicker.SelectedIndex)
        {
            case 1:
                query = query.Where(x => x.IsActive);
                break;
            case 2:
                query = query.Where(x => !x.IsActive);
                break;
        }

        query = SortPicker.SelectedIndex switch
        {
            1 => query.OrderBy(x => x.CreatedAtUtc),
            2 => query.OrderBy(x => x.Name),
            3 => query.OrderByDescending(x => x.Name),
            _ => query.OrderByDescending(x => x.CreatedAtUtc)
        };

        foreach (var store in query)
            Stores.Add(new StoreListItemViewModel(store));

        UpdateFiltersSummary();
    }

    private void SetFiltersVisibility(bool visible)
    {
        _filtersVisible = visible;
        FiltersPanel.IsVisible = visible;
        ToggleFiltersButton.Text = visible ? "إخفاء ▲" : "إظهار ▼";
        UpdateFiltersSummary();
    }

    private void UpdateFiltersSummary()
    {
        var parts = new List<string>();

        var searchText = SearchEntry.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(searchText))
            parts.Add($"بحث: {searchText}");

        var selectedType = StoreTypeFilterPicker.SelectedItem as StoreTypeFilterItem;
        if (selectedType != null && selectedType.Id > 0)
            parts.Add($"النوع: {selectedType.Name}");

        if (StatusFilterPicker.SelectedIndex == 1)
            parts.Add("الحالة: نشط فقط");
        else if (StatusFilterPicker.SelectedIndex == 2)
            parts.Add("الحالة: غير نشط فقط");

        if (SortPicker.SelectedItem is string sortText && !string.IsNullOrWhiteSpace(sortText))
            parts.Add($"الترتيب: {sortText}");

        if (parts.Count == 0)
            FiltersSummaryLabel.Text = _filtersVisible ? "لا توجد فلاتر مفعلة" : "الفلاتر مخفية";
        else
            FiltersSummaryLabel.Text = string.Join(" | ", parts);
    }

    private async Task GoToEditAsync(StoreListItemViewModel? item)
    {
        if (item == null || item.Id <= 0 || !CanUpdateStore)
            return;

        await Shell.Current.GoToAsync($"{AppShell.RouteEditStore}?storeId={item.Id}");
    }

    private async Task DeleteStoreAsync(StoreListItemViewModel? item)
    {
        if (_isBusy || item == null || item.Id <= 0 || !CanDeleteStore)
            return;

        var confirm = await DisplayAlert(
            "تأكيد الحذف",
            $"سيتم حذف المتجر \"{item.Name}\" حذفًا عميقًا مع جميع المستخدمين والمنتجات والصور والبيانات المرتبطة به. هل تريد المتابعة؟",
            "نعم",
            "إلغاء");

        if (!confirm)
            return;

        try
        {
            _isBusy = true;
            SetLoading(true);
            ShowMessage(null);

            var result = await _storesService.DeleteStoreAsync(item.Id);

            if (!result.IsSuccess)
            {
                ShowMessage(result.Message);
                return;
            }

            var existingItem = _allStores.FirstOrDefault(x => x.Id == item.Id);
            if (existingItem != null)
                _allStores.Remove(existingItem);

            ApplyFilters();

            await DisplayAlert("نجاح", result.Message, "موافق");
        }
        catch (Exception ex)
        {
            ShowMessage($"تعذر حذف المتجر: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
            _isBusy = false;
        }
    }

    private async void OnAddStoreClicked(object sender, EventArgs e)
    {
        if (!CanCreateStore)
            return;

        await Shell.Current.GoToAsync(AppShell.RouteAddStore);
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadStoreTypesAsync();
        await LoadStoresAsync();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(AppShell.RouteDashboard);
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void OnFilterChanged(object sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void OnClearFiltersClicked(object sender, EventArgs e)
    {
        SearchEntry.Text = string.Empty;
        if (StoreTypeFilterPicker.ItemsSource != null)
            StoreTypeFilterPicker.SelectedIndex = 0;
        StatusFilterPicker.SelectedIndex = 0;
        SortPicker.SelectedIndex = 0;
        ApplyFilters();
    }

    private void OnToggleFiltersClicked(object sender, EventArgs e)
    {
        SetFiltersVisibility(!_filtersVisible);
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        StoresCollectionView.IsVisible = !isLoading;
    }

    private void ShowMessage(string? message)
    {
        MessageLabel.Text = message ?? string.Empty;
        MessageLabel.IsVisible = !string.IsNullOrWhiteSpace(message);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public sealed class StoreTypeFilterItem
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed class StoreListItemViewModel
    {
        public long Id { get; }
        public long? StoreTypeId { get; }
        public string Name { get; }
        public string Slug { get; }
        public string Description { get; }
        public string Phone { get; }
        public string LogoPath { get; }
        public bool IsActive { get; }
        public string IsActiveText => IsActive ? "نشط" : "غير نشط";
        public string CreatedAtText { get; }
        public string StoreTypeName { get; }
        public string StoreTypeCode { get; }

        public StoreListItemViewModel(StoreDto dto)
        {
            Id = dto.Id;
            StoreTypeId = dto.StoreTypeId;
            Name = dto.Name ?? string.Empty;
            Slug = string.IsNullOrWhiteSpace(dto.Slug) ? "—" : dto.Slug;
            Description = string.IsNullOrWhiteSpace(dto.Description) ? "لا يوجد وصف" : dto.Description;
            Phone = "—";
            StoreTypeName = string.IsNullOrWhiteSpace(dto.StoreTypeName) ? "بدون نوع" : $"النوع: {dto.StoreTypeName}";
            StoreTypeCode = string.IsNullOrWhiteSpace(dto.StoreTypeCode) ? string.Empty : $"الكود: {dto.StoreTypeCode}";

            if (!string.IsNullOrWhiteSpace(dto.LogoPath))
                LogoPath = $"{ApiConfig.BaseUrl.TrimEnd('/')}{dto.LogoPath}";
            else
                LogoPath = string.Empty;

            IsActive = dto.IsActive;
            CreatedAtText = dto.CreatedAtUtc.ToLocalTime().ToString("yyyy/MM/dd hh:mm tt");
        }
    }
}