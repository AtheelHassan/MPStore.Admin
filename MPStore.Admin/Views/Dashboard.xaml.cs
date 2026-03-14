using System.ComponentModel;
using System.Runtime.CompilerServices;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views
{
    public partial class Dashboard : ContentPage, INotifyPropertyChanged
    {
        private readonly StoresService _storesService;
        private readonly AdminUsersService _adminUsersService;
        private readonly AuthService _authService;

        private bool _isBusy;

        private bool _canViewStores;
        public bool CanViewStores
        {
            get => _canViewStores;
            set
            {
                if (_canViewStores == value) return;
                _canViewStores = value;
                OnPropertyChanged();
            }
        }

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

        public new event PropertyChangedEventHandler? PropertyChanged;

        public Dashboard(
            StoresService storesService,
            AdminUsersService adminUsersService,
            AuthService authService)
        {
            InitializeComponent();
            _storesService = storesService;
            _adminUsersService = adminUsersService;
            _authService = authService;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDashboardAsync();
        }

        private async Task LoadDashboardAsync()
        {
            if (_isBusy)
                return;

            try
            {
                SetBusy(true);

                var session = await _authService.GetSavedSessionAsync();

                WelcomeLabel.Text = string.IsNullOrWhiteSpace(session?.DisplayName)
                    ? $"مرحباً {session?.Username}"
                    : $"مرحباً {session?.DisplayName}";

                RoleLabel.Text = string.IsNullOrWhiteSpace(session?.AdminRoleName)
                    ? "جاهز لإدارة النظام"
                    : $"الدور: {session.AdminRoleName}";

                CurrentUserInfoLabel.Text = string.IsNullOrWhiteSpace(session?.Email)
                    ? $"المستخدم: {session?.Username}"
                    : $"المستخدم: {session?.Username} - {session?.Email}";

                CurrentPermissionsInfoLabel.Text = $"الصلاحيات: {session?.Permissions?.Count ?? 0}";

                CanViewStores =
                    await _authService.HasPermissionAsync("stores.view") ||
                    await _authService.HasPermissionAsync("stores.create") ||
                    await _authService.HasPermissionAsync("stores.update") ||
                    await _authService.HasPermissionAsync("stores.delete");

                CanViewAdminUsers = await _authService.HasPermissionAsync("admin_users.view");
                CanViewAdminRoles = await _authService.HasPermissionAsync("admin_roles.view");

                var stores = await _storesService.GetStoresAsync();
                var admins = await _adminUsersService.GetAllAsync();

                var storesCount = stores?.Count ?? 0;
                var activeStoresCount = stores?.Count(x => x.IsActive) ?? 0;

                var adminsCount = admins?.Count ?? 0;
                var activeAdminsCount = admins?.Count(x => x.IsActive) ?? 0;

                StoresCountLabel.Text = storesCount.ToString();
                ActiveStoresCountLabel.Text = activeStoresCount.ToString();
                AdminsCountLabel.Text = adminsCount.ToString();
                ActiveAdminsCountLabel.Text = activeAdminsCount.ToString();
            }
            catch
            {
                StoresCountLabel.Text = "0";
                ActiveStoresCountLabel.Text = "0";
                AdminsCountLabel.Text = "0";
                ActiveAdminsCountLabel.Text = "0";
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void OnStoresTapped(object sender, TappedEventArgs e)
        {
            if (!CanViewStores)
                return;

            await Shell.Current.GoToAsync(AppShell.RouteStoresList);
        }

        private async void OnAdminUsersTapped(object sender, TappedEventArgs e)
        {
            if (!CanViewAdminUsers)
                return;

            await Shell.Current.GoToAsync(AppShell.RouteAdminUsersList);
        }

        private async void OnAdminRolesTapped(object sender, TappedEventArgs e)
        {
            if (!CanViewAdminRoles)
                return;

            await Shell.Current.GoToAsync(AppShell.RouteAdminRolesList);
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            if (_isBusy)
                return;

            var confirm = await DisplayAlert("تأكيد", "هل تريد تسجيل الخروج؟", "نعم", "إلغاء");
            if (!confirm)
                return;

            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync("//Login");
        }

        private void SetBusy(bool value)
        {
            _isBusy = value;
            BusyIndicator.IsVisible = value;
            BusyIndicator.IsRunning = value;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}