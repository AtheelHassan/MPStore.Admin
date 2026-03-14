using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MPStore.Admin.Models.StoreUsers;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views;

[QueryProperty(nameof(StoreId), "storeId")]
public partial class StoreUsersList : ContentPage, INotifyPropertyChanged
{
    private readonly StoreUsersService _service;
    private readonly AuthService _authService;
    private long _storeId;
    private bool _isBusy;

    private bool _canViewStoreUsers;
    private bool _canCreateStoreUsers;
    private bool _canUpdateStoreUsers;
    private bool _canDeleteStoreUsers;

    public ObservableCollection<StoreUserDto> Users { get; set; } = new();

    public bool CanViewStoreUsers
    {
        get => _canViewStoreUsers;
        set
        {
            if (_canViewStoreUsers == value) return;
            _canViewStoreUsers = value;
            OnPropertyChanged();
        }
    }

    public bool CanCreateStoreUsers
    {
        get => _canCreateStoreUsers;
        set
        {
            if (_canCreateStoreUsers == value) return;
            _canCreateStoreUsers = value;
            OnPropertyChanged();
        }
    }

    public bool CanUpdateStoreUsers
    {
        get => _canUpdateStoreUsers;
        set
        {
            if (_canUpdateStoreUsers == value) return;
            _canUpdateStoreUsers = value;
            OnPropertyChanged();
        }
    }

    public bool CanDeleteStoreUsers
    {
        get => _canDeleteStoreUsers;
        set
        {
            if (_canDeleteStoreUsers == value) return;
            _canDeleteStoreUsers = value;
            OnPropertyChanged();
        }
    }

    public string StoreId
    {
        get => _storeId.ToString();
        set
        {
            if (long.TryParse(value, out var id))
                _storeId = id;
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    public StoreUsersList(StoreUsersService service, AuthService authService)
    {
        InitializeComponent();
        _service = service;
        _authService = authService;
        BindingContext = this;
        UsersCollectionView.ItemsSource = Users;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadPermissionsAsync();

        if (!CanViewStoreUsers)
        {
            Users.Clear();
            UsersCollectionView.IsVisible = false;
            AddUserButton.IsVisible = false;
            MessageLabel.Text = "ليس لديك صلاحية لعرض العاملين.";
            MessageLabel.IsVisible = true;
            return;
        }

        UsersCollectionView.IsVisible = true;
        AddUserButton.IsVisible = CanCreateStoreUsers;

        await LoadUsers();
    }

    private async Task LoadPermissionsAsync()
    {
        try
        {
            CanViewStoreUsers = await _authService.HasPermissionAsync("store_users.view");
            CanCreateStoreUsers = await _authService.HasPermissionAsync("store_users.create");
            CanUpdateStoreUsers = await _authService.HasPermissionAsync("store_users.update");
            CanDeleteStoreUsers = await _authService.HasPermissionAsync("store_users.delete");
        }
        catch
        {
            CanViewStoreUsers = false;
            CanCreateStoreUsers = false;
            CanUpdateStoreUsers = false;
            CanDeleteStoreUsers = false;
        }
    }

    private async Task LoadUsers()
    {
        if (_isBusy || _storeId <= 0 || !CanViewStoreUsers)
            return;

        try
        {
            _isBusy = true;

            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            MessageLabel.IsVisible = false;

            Users.Clear();

            var list = await _service.GetStoreUsersAsync(_storeId);

            if (list != null)
            {
                foreach (var u in list)
                    Users.Add(u);
            }
        }
        catch (Exception ex)
        {
            MessageLabel.Text = ex.Message;
            MessageLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            _isBusy = false;
        }
    }

    private async void OnAddUserClicked(object sender, EventArgs e)
    {
        if (!CanCreateStoreUsers)
            return;

        await Shell.Current.GoToAsync($"{AppShell.RouteAddStoreUser}?storeId={_storeId}");
    }

    private async void OnEdit(object sender, EventArgs e)
    {
        if (!CanUpdateStoreUsers)
            return;

        var btn = sender as Button;
        var user = btn?.BindingContext as StoreUserDto;

        if (user == null)
            return;

        await Shell.Current.GoToAsync($"{AppShell.RouteEditStoreUser}?userId={user.Id}&storeId={_storeId}");
    }

    private async void OnDelete(object sender, EventArgs e)
    {
        if (!CanDeleteStoreUsers)
            return;

        var btn = sender as Button;
        var user = btn?.BindingContext as StoreUserDto;

        if (user == null)
            return;

        bool confirm = await DisplayAlert("تأكيد", "حذف المستخدم؟", "نعم", "إلغاء");
        if (!confirm)
            return;

        MessageLabel.Text = "دالة الحذف غير موجودة بعد في StoreUsersService.";
        MessageLabel.IsVisible = true;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(AppShell.RouteDashboard);
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadUsers();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}