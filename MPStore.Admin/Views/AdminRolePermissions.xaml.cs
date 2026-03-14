using MPStore.Admin.Models.AdminRoles;
using MPStore.Admin.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MPStore.Admin.Views;

[QueryProperty(nameof(RoleId), "roleId")]
public partial class AdminRolePermissions : ContentPage
{
    private readonly AdminRolesService _service;
    private long _roleId;
    private bool _isBusy;
    private bool _isLoaded;

    public ObservableCollection<PermissionItemViewModel> Permissions { get; } = new();

    public string RoleId
    {
        get => _roleId.ToString();
        set
        {
            if (long.TryParse(value, out var id))
                _roleId = id;
        }
    }

    public AdminRolePermissions(AdminRolesService service)
    {
        InitializeComponent();
        _service = service;
        BindingContext = this;
        PermissionsCollectionView.ItemsSource = Permissions;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isLoaded || _isBusy || _roleId <= 0)
            return;

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (_isBusy || _roleId <= 0)
            return;

        try
        {
            _isBusy = true;
            SetLoading(true);
            ShowMessage(null);

            Permissions.Clear();

            var role = await _service.GetByIdAsync(_roleId);
            if (role == null)
            {
                ShowMessage("تعذر العثور على الدور المطلوب.");
                return;
            }

            RoleNameLabel.Text = $"صلاحيات الدور: {role.Name}";

            var existingPermissions = await _service.GetPermissionsAsync(_roleId);
            var allowedCodes = existingPermissions?
                .Where(x => x.IsAllowed)
                .Select(x => x.PermissionCode?.Trim()?.ToLowerInvariant())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in GetPermissionCatalog())
            {
                item.IsAllowed = allowedCodes.Contains(item.PermissionCode);
                Permissions.Add(item);
            }

            _isLoaded = true;
        }
        catch (Exception ex)
        {
            ShowMessage($"تعذر تحميل الصلاحيات: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
            _isBusy = false;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_isBusy || _roleId <= 0)
            return;

        try
        {
            _isBusy = true;
            SetLoading(true);
            ShowMessage(null);

            var request = Permissions
                .Select(x => new AdminRolePermissionItemRequest
                {
                    PermissionCode = x.PermissionCode,
                    IsAllowed = x.IsAllowed
                })
                .ToList();

            var ok = await _service.UpdatePermissionsAsync(_roleId, request);

            if (!ok)
            {
                ShowMessage("فشل حفظ الصلاحيات.");
                return;
            }

            await DisplayAlert("نجاح", "تم حفظ صلاحيات الدور بنجاح.", "موافق");
            await Shell.Current.GoToAsync(AppShell.RouteAdminRolesList);
        }
        catch (Exception ex)
        {
            ShowMessage($"حدث خطأ أثناء الحفظ: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
            _isBusy = false;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(AppShell.RouteAdminRolesList);
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        PermissionsCollectionView.IsVisible = !isLoading;
    }

    private void ShowMessage(string? message)
    {
        MessageLabel.Text = message ?? string.Empty;
        MessageLabel.IsVisible = !string.IsNullOrWhiteSpace(message);
    }

    private static List<PermissionItemViewModel> GetPermissionCatalog()
    {
        return new List<PermissionItemViewModel>
        {
            new("stores.view", "عرض المتاجر"),
            new("stores.create", "إنشاء متجر"),
            new("stores.update", "تعديل متجر"),
            new("stores.delete", "حذف متجر"),
            new("stores.disable", "تعطيل متجر"),

            new("store_users.view", "عرض مستخدمي المتاجر"),
            new("store_users.create", "إنشاء مستخدم متجر"),
            new("store_users.update", "تعديل مستخدم متجر"),
            new("store_users.delete", "حذف مستخدم متجر"),
            new("store_users.disable", "تعطيل مستخدم متجر"),

            new("store_roles.view", "عرض أدوار المتاجر"),
            new("store_roles.create", "إنشاء دور متجر"),
            new("store_roles.update", "تعديل دور متجر"),
            new("store_roles.delete", "حذف دور متجر"),
            new("store_roles.disable", "تعطيل دور متجر"),

            new("admin_users.view", "عرض الأدمن"),
            new("admin_users.create", "إنشاء أدمن"),
            new("admin_users.update", "تعديل أدمن"),
            new("admin_users.delete", "حذف أدمن"),

            new("admin_roles.view", "عرض الأدوار الإدارية"),
            new("admin_roles.create", "إنشاء دور إداري"),
            new("admin_roles.update", "تعديل دور إداري"),
            new("admin_roles.delete", "حذف دور إداري"),

            new("audit_logs.view", "عرض سجل العمليات")
        };
    }

    public sealed class PermissionItemViewModel : INotifyPropertyChanged
    {
        private bool _isAllowed;

        public string PermissionCode { get; }
        public string Description { get; }

        public bool IsAllowed
        {
            get => _isAllowed;
            set
            {
                if (_isAllowed == value)
                    return;

                _isAllowed = value;
                OnPropertyChanged();
            }
        }

        public PermissionItemViewModel(string permissionCode, string description)
        {
            PermissionCode = permissionCode;
            Description = description;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}