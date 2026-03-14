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

    private bool _canViewAdminUsers;
    public bool CanViewAdminUsers
    {
        get => _canViewAdminUsers;
        set
        {
            if (_canViewAdminUsers == value) return;
            _canViewAdminUsers = value;
            OnPropertyChanged();
        }
    }

    private bool _canViewAdminRoles;
    public bool CanViewAdminRoles
    {
        get => _canViewAdminRoles;
        set
        {
            if (_canViewAdminRoles == value) return;
            _canViewAdminRoles = value;
            OnPropertyChanged();
        }
    }

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
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await EnsurePermissionsLoadedAsync();
        await LoadStoresAsync();
    }

    private async Task EnsurePermissionsLoadedAsync()
    {
        if (_permissionsLoaded)
            return;

        CanViewAdminUsers = await _authService.HasPermissionAsync("admin_users.view");
        CanViewAdminRoles = await _authService.HasPermissionAsync("admin_roles.view");
        CanCreateStore = await _authService.HasPermissionAsync("stores.create");
        CanUpdateStore = await _authService.HasPermissionAsync("stores.update");
        CanDeleteStore = await _authService.HasPermissionAsync("stores.delete");

        _permissionsLoaded = true;
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

            var result = await _storesService.GetStoresAsync();

            if (result == null || result.Count == 0)
                return;

            foreach (var store in result)
                Stores.Add(new StoreListItemViewModel(store));
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

            var existingItem = Stores.FirstOrDefault(x => x.Id == item.Id);
            if (existingItem != null)
                Stores.Remove(existingItem);

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
        await LoadStoresAsync();
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        try
        {
            bool confirm = await DisplayAlert("تأكيد", "هل تريد تسجيل الخروج؟", "نعم", "إلغاء");
            if (!confirm)
                return;

            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync("//Login");
        }
        catch
        {
        }
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

    public sealed class StoreListItemViewModel
    {
        public long Id { get; }
        public string Name { get; }
        public string Slug { get; }
        public string Description { get; }
        public string Phone { get; }
        public string LogoPath { get; }
        public bool IsActive { get; }
        public string IsActiveText => IsActive ? "نشط" : "غير نشط";
        public string CreatedAtText { get; }

        public StoreListItemViewModel(StoreDto dto)
        {
            Id = dto.Id;
            Name = dto.Name ?? string.Empty;
            Slug = dto.Slug ?? string.Empty;
            Description = string.IsNullOrWhiteSpace(dto.Description) ? "لا يوجد وصف" : dto.Description;
            Phone = "—";

            if (!string.IsNullOrWhiteSpace(dto.LogoPath))
                LogoPath = $"{ApiConfig.BaseUrl.TrimEnd('/')}{dto.LogoPath}";
            else
                LogoPath = string.Empty;

            IsActive = dto.IsActive;
            CreatedAtText = dto.CreatedAtUtc.ToLocalTime().ToString("yyyy/MM/dd hh:mm tt");
        }
    }

    private async void OnAdminUsersClicked(object sender, EventArgs e)
    {
        if (!CanViewAdminUsers)
            return;

        await Shell.Current.GoToAsync(AppShell.RouteAdminUsersList);
    }

    private async void OnAdminRolesClicked(object sender, EventArgs e)
    {
        if (!CanViewAdminRoles)
            return;

        await Shell.Current.GoToAsync(AppShell.RouteAdminRolesList);
    }
}