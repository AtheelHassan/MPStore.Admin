using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MPStore.Admin.Models.AdminUsers;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views;

public partial class AdminUsersList : ContentPage, INotifyPropertyChanged
{
    private readonly AdminUsersService _adminUsersService;
    private readonly AuthService _authService;
    private bool _isBusy;
    private bool _permissionsLoaded;

    private bool _canCreateAdminUser;
    public bool CanCreateAdminUser
    {
        get => _canCreateAdminUser;
        set
        {
            if (_canCreateAdminUser == value) return;
            _canCreateAdminUser = value;
            RaisePropertyChanged();
        }
    }

    private bool _canUpdateAdminUser;
    public bool CanUpdateAdminUser
    {
        get => _canUpdateAdminUser;
        set
        {
            if (_canUpdateAdminUser == value) return;
            _canUpdateAdminUser = value;
            RaisePropertyChanged();
        }
    }

    private bool _canDeleteAdminUser;
    public bool CanDeleteAdminUser
    {
        get => _canDeleteAdminUser;
        set
        {
            if (_canDeleteAdminUser == value) return;
            _canDeleteAdminUser = value;
            RaisePropertyChanged();
        }
    }

    public ObservableCollection<AdminUserItemViewModel> AdminUsers { get; } = new();

    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ToggleActiveCommand { get; }

    public new event PropertyChangedEventHandler? PropertyChanged;

    public AdminUsersList(AdminUsersService adminUsersService, AuthService authService)
    {
        InitializeComponent();

        _adminUsersService = adminUsersService;
        _authService = authService;
        BindingContext = this;

        AdminsCollectionView.ItemsSource = AdminUsers;

        EditCommand = new Command<AdminUserItemViewModel>(async item => await EditAdminAsync(item));
        DeleteCommand = new Command<AdminUserItemViewModel>(async item => await DeleteAdminAsync(item));
        ToggleActiveCommand = new Command<AdminUserItemViewModel>(async item => await ToggleActiveAsync(item));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await EnsurePermissionsLoadedAsync();
        await LoadAdminsAsync();
    }

    private async Task EnsurePermissionsLoadedAsync()
    {
        if (_permissionsLoaded)
            return;

        CanCreateAdminUser = await _authService.HasPermissionAsync("admin_users.create");
        CanUpdateAdminUser = await _authService.HasPermissionAsync("admin_users.update");
        CanDeleteAdminUser = await _authService.HasPermissionAsync("admin_users.delete");

        _permissionsLoaded = true;
    }

    private async Task LoadAdminsAsync()
    {
        if (_isBusy)
            return;

        try
        {
            _isBusy = true;

            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            MessageLabel.IsVisible = false;

            AdminUsers.Clear();

            var result = await _adminUsersService.GetAllAsync();

            if (result == null)
                return;

            foreach (var admin in result)
                AdminUsers.Add(new AdminUserItemViewModel(admin));
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("خطأ", ex.Message, "موافق");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            _isBusy = false;
        }
    }

    private async Task EditAdminAsync(AdminUserItemViewModel? item)
    {
        if (item == null || !CanUpdateAdminUser)
            return;

        await Shell.Current.GoToAsync($"{AppShell.RouteEditAdminUser}?userId={item.Id}");
    }

    private async Task DeleteAdminAsync(AdminUserItemViewModel? item)
    {
        if (item == null || !CanDeleteAdminUser)
            return;

        var confirm = await DisplayAlertAsync(
            "تأكيد",
            $"هل تريد حذف الأدمن {item.Username} ؟",
            "نعم",
            "إلغاء");

        if (!confirm)
            return;

        var success = await _adminUsersService.DeleteAsync(item.Id);

        if (success)
        {
            AdminUsers.Remove(item);
        }
        else
        {
            await DisplayAlertAsync("خطأ", "فشل حذف الأدمن", "موافق");
        }
    }

    private async Task ToggleActiveAsync(AdminUserItemViewModel? item)
    {
        if (item == null || !CanUpdateAdminUser)
            return;

        var newState = !item.IsActive;

        var success = await _adminUsersService.SetActiveAsync(item.Id, newState);

        if (success)
        {
            item.SetIsActive(newState);
        }
        else
        {
            await DisplayAlertAsync("خطأ", "فشل تحديث الحالة", "موافق");
        }
    }

    private async void OnAddAdminClicked(object sender, EventArgs e)
    {
        if (!CanCreateAdminUser)
            return;

        await Shell.Current.GoToAsync(AppShell.RouteAddAdminUser);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(AppShell.RouteStoresList);
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadAdminsAsync();
    }

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static Task DisplayAlertAsync(string title, string message, string cancel)
    {
        return Application.Current?.Windows.FirstOrDefault()?.Page?.DisplayAlert(title, message, cancel)
               ?? Task.CompletedTask;
    }

    public sealed class AdminUserItemViewModel : INotifyPropertyChanged
    {
        public long Id { get; }
        public string Username { get; }
        public string DisplayName { get; }
        public string Email { get; }
        public string RoleName { get; }
        public DateTime CreatedAtUtc { get; }

        private bool _isActive;

        public bool IsActive
        {
            get => _isActive;
            private set
            {
                _isActive = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(StatusText));
                RaisePropertyChanged(nameof(StatusBackgroundColor));
                RaisePropertyChanged(nameof(StatusTextColor));
            }
        }

        public string StatusText => IsActive ? "نشط" : "موقوف";
        public string StatusBackgroundColor => IsActive ? "#E5F7EB" : "#FEE2E2";
        public string StatusTextColor => IsActive ? "#15803D" : "#B91C1C";
        public string CreatedAtText => CreatedAtUtc.ToLocalTime().ToString("yyyy/MM/dd HH:mm");

        public event PropertyChangedEventHandler? PropertyChanged;

        public AdminUserItemViewModel(AdminUserDto dto)
        {
            Id = dto.Id;
            Username = dto.Username ?? string.Empty;
            DisplayName = string.IsNullOrWhiteSpace(dto.DisplayName) ? "بدون اسم عرض" : dto.DisplayName!;
            Email = string.IsNullOrWhiteSpace(dto.Email) ? "لا يوجد بريد إلكتروني" : dto.Email!;
            RoleName = $"Role #{dto.Role}";
            CreatedAtUtc = dto.CreatedAtUtc;
            _isActive = dto.IsActive;
        }

        public void SetIsActive(bool value)
        {
            IsActive = value;
        }

        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}