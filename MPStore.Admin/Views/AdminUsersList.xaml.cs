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
    private bool _isBusy;

    public ObservableCollection<AdminUserItemViewModel> AdminUsers { get; } = new();

    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ToggleActiveCommand { get; }

    public new event PropertyChangedEventHandler? PropertyChanged;

    public AdminUsersList(AdminUsersService adminUsersService)
    {
        InitializeComponent();

        _adminUsersService = adminUsersService;
        BindingContext = this;

        AdminsCollectionView.ItemsSource = AdminUsers;

        EditCommand = new Command<AdminUserItemViewModel>(async item => await EditAdminAsync(item));
        DeleteCommand = new Command<AdminUserItemViewModel>(async item => await DeleteAdminAsync(item));
        ToggleActiveCommand = new Command<AdminUserItemViewModel>(async item => await ToggleActiveAsync(item));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAdminsAsync();
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

            AdminUsers.Clear();

            var result = await _adminUsersService.GetAllAsync();

            if (result == null)
                return;

            foreach (var admin in result)
                AdminUsers.Add(new AdminUserItemViewModel(admin));
        }
        catch (Exception ex)
        {
            await DisplayAlert("خطأ", ex.Message, "موافق");
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
        if (item == null) return;

        await Shell.Current.GoToAsync($"{AppShell.RouteEditAdminUser}?id={item.Id}");
    }

    private async Task DeleteAdminAsync(AdminUserItemViewModel? item)
    {
        if (item == null) return;

        var confirm = await DisplayAlert(
            "تأكيد",
            $"هل تريد حذف الأدمن {item.Username} ؟",
            "نعم",
            "إلغاء");

        if (!confirm) return;

        var success = await _adminUsersService.DeleteAsync(item.Id);

        if (success)
        {
            AdminUsers.Remove(item);
        }
        else
        {
            await DisplayAlert("خطأ", "فشل حذف الأدمن", "موافق");
        }
    }

    private async Task ToggleActiveAsync(AdminUserItemViewModel? item)
    {
        if (item == null) return;

        var newState = !item.IsActive;

        var success = await _adminUsersService.SetActiveAsync(item.Id, newState);

        if (success)
        {
            item.SetIsActive(newState);
        }
        else
        {
            await DisplayAlert("خطأ", "فشل تحديث الحالة", "موافق");
        }
    }

    private async void OnAddAdminClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(AppShell.RouteAddAdminUser);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadAdminsAsync();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public sealed class AdminUserItemViewModel : INotifyPropertyChanged
    {
        public long Id { get; }
        public string Username { get; }
        public DateTime CreatedAtUtc { get; }

        private bool _isActive;

        public bool IsActive
        {
            get => _isActive;
            private set
            {
                _isActive = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public string StatusText => IsActive ? "نشط" : "موقوف";

        public string CreatedAtText =>
            CreatedAtUtc.ToLocalTime().ToString("yyyy/MM/dd HH:mm");

        public event PropertyChangedEventHandler? PropertyChanged;

        public AdminUserItemViewModel(AdminUserDto dto)
        {
            Id = dto.Id;
            Username = dto.Username ?? "";
            CreatedAtUtc = dto.CreatedAtUtc;
            _isActive = dto.IsActive;
        }

        public void SetIsActive(bool value)
        {
            IsActive = value;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}