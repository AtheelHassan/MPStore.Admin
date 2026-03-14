using MPStore.Admin.Models.AdminRoles;
using MPStore.Admin.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MPStore.Admin.Views
{
    public partial class AdminRolesList : ContentPage, INotifyPropertyChanged
    {
        private readonly AdminRolesService _service;
        private readonly AuthService _authService;
        private bool _isBusy;

        private bool _canCreateAdminRole;
        private bool _canUpdateAdminRole;
        private bool _canDeleteAdminRole;
        private bool _canManageAdminRolePermissions;

        public ObservableCollection<AdminRoleItemViewModel> Roles { get; } = new();

        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand PermissionsCommand { get; }

        public bool CanCreateAdminRole
        {
            get => _canCreateAdminRole;
            set
            {
                if (_canCreateAdminRole == value) return;
                _canCreateAdminRole = value;
                OnPropertyChanged();
            }
        }

        public bool CanUpdateAdminRole
        {
            get => _canUpdateAdminRole;
            set
            {
                if (_canUpdateAdminRole == value) return;
                _canUpdateAdminRole = value;
                OnPropertyChanged();
            }
        }

        public bool CanDeleteAdminRole
        {
            get => _canDeleteAdminRole;
            set
            {
                if (_canDeleteAdminRole == value) return;
                _canDeleteAdminRole = value;
                OnPropertyChanged();
            }
        }

        public bool CanManageAdminRolePermissions
        {
            get => _canManageAdminRolePermissions;
            set
            {
                if (_canManageAdminRolePermissions == value) return;
                _canManageAdminRolePermissions = value;
                OnPropertyChanged();
            }
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        public AdminRolesList(AdminRolesService service, AuthService authService)
        {
            InitializeComponent();

            _service = service;
            _authService = authService;
            BindingContext = this;

            RolesCollectionView.ItemsSource = Roles;

            EditCommand = new Command<AdminRoleItemViewModel>(async item => await EditRoleAsync(item));
            DeleteCommand = new Command<AdminRoleItemViewModel>(async item => await DeleteRoleAsync(item));
            PermissionsCommand = new Command<AdminRoleItemViewModel>(async item => await OpenPermissionsAsync(item));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await LoadPermissionsAsync();
            await LoadRolesAsync();
        }

        private async Task LoadPermissionsAsync()
        {
            try
            {
                CanCreateAdminRole = await _authService.HasPermissionAsync("admin_roles.create");
                CanUpdateAdminRole = await _authService.HasPermissionAsync("admin_roles.update");
                CanDeleteAdminRole = await _authService.HasPermissionAsync("admin_roles.delete");

                var canViewPermissions = await _authService.HasPermissionAsync("admin_role_permissions.view");
                var canUpdatePermissions = await _authService.HasPermissionAsync("admin_role_permissions.update");
                CanManageAdminRolePermissions = canViewPermissions || canUpdatePermissions;
            }
            catch
            {
                CanCreateAdminRole = false;
                CanUpdateAdminRole = false;
                CanDeleteAdminRole = false;
                CanManageAdminRolePermissions = false;
            }
        }

        private async Task LoadRolesAsync()
        {
            if (_isBusy)
                return;

            try
            {
                _isBusy = true;
                SetLoading(true);
                ShowMessage(null);

                Roles.Clear();

                var list = await _service.GetAllAsync();

                if (list == null || list.Count == 0)
                    return;

                foreach (var role in list)
                    Roles.Add(new AdminRoleItemViewModel(role));
            }
            catch (Exception ex)
            {
                ShowMessage($"تعذر تحميل الأدوار: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
                _isBusy = false;
            }
        }

        private async Task EditRoleAsync(AdminRoleItemViewModel? item)
        {
            if (item == null || !CanUpdateAdminRole)
                return;

            await Shell.Current.GoToAsync($"{AppShell.RouteEditAdminRole}?roleId={item.Id}");
        }

        private async Task DeleteRoleAsync(AdminRoleItemViewModel? item)
        {
            if (item == null || _isBusy || !CanDeleteAdminRole)
                return;

            bool confirm = await DisplayAlert(
                "تأكيد",
                $"هل تريد حذف الدور \"{item.Name}\" ؟",
                "نعم",
                "إلغاء");

            if (!confirm)
                return;

            try
            {
                _isBusy = true;
                SetLoading(true);

                var ok = await _service.DeleteAsync(item.Id);

                if (!ok)
                {
                    ShowMessage("فشل حذف الدور.");
                    return;
                }

                var existing = Roles.FirstOrDefault(x => x.Id == item.Id);
                if (existing != null)
                    Roles.Remove(existing);
            }
            catch (Exception ex)
            {
                ShowMessage($"خطأ: {ex.Message}");
            }
            finally
            {
                _isBusy = false;
                SetLoading(false);
            }
        }

        private async Task OpenPermissionsAsync(AdminRoleItemViewModel? item)
        {
            if (item == null || !CanManageAdminRolePermissions)
                return;

            await Shell.Current.GoToAsync($"{AppShell.RouteAdminRolePermissions}?roleId={item.Id}");
        }

        private async void OnAddRoleClicked(object sender, EventArgs e)
        {
            if (!CanCreateAdminRole)
                return;

            await Shell.Current.GoToAsync(AppShell.RouteAddAdminRole);
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(AppShell.RouteDashboard);
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadRolesAsync();
        }

        private void SetLoading(bool value)
        {
            LoadingIndicator.IsVisible = value;
            LoadingIndicator.IsRunning = value;
            RolesCollectionView.IsVisible = !value;
        }

        private void ShowMessage(string? msg)
        {
            MessageLabel.Text = msg ?? "";
            MessageLabel.IsVisible = !string.IsNullOrWhiteSpace(msg);
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public sealed class AdminRoleItemViewModel
        {
            public long Id { get; }
            public string Name { get; }
            public string Code { get; }
            public string Description { get; }
            public bool IsActive { get; }
            public bool IsSystemRole { get; }
            public DateTime CreatedAtUtc { get; }

            public string StatusText => IsActive ? "نشط" : "معطل";
            public string StatusBackgroundColor => IsActive ? "#E5F7EB" : "#FEE2E2";
            public string StatusTextColor => IsActive ? "#15803D" : "#B91C1C";
            public string TypeText => IsSystemRole ? "دور نظام" : "دور مخصص";
            public string CreatedAtText => CreatedAtUtc.ToLocalTime().ToString("yyyy/MM/dd hh:mm tt");

            public AdminRoleItemViewModel(AdminRoleDto dto)
            {
                Id = dto.Id;
                Name = dto.Name ?? "";
                Code = dto.Code ?? "";
                Description = string.IsNullOrWhiteSpace(dto.Description) ? "لا يوجد وصف" : dto.Description;
                IsActive = dto.IsActive;
                IsSystemRole = dto.IsSystemRole;
                CreatedAtUtc = dto.CreatedAtUtc;
            }
        }
    }
}