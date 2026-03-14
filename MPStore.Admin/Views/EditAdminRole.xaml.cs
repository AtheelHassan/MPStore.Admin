using MPStore.Admin.Models.AdminRoles;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views;

[QueryProperty(nameof(RoleId), "roleId")]
public partial class EditAdminRole : ContentPage
{
    private readonly AdminRolesService _service;
    private long _roleId;
    private bool _isBusy;
    private bool _isLoaded;

    public string RoleId
    {
        get => _roleId.ToString();
        set
        {
            if (long.TryParse(value, out var id))
                _roleId = id;
        }
    }

    public EditAdminRole(AdminRolesService service)
    {
        InitializeComponent();
        _service = service;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isLoaded || _isBusy || _roleId <= 0)
            return;

        await LoadRoleAsync();
    }

    private async Task LoadRoleAsync()
    {
        if (_isBusy || _roleId <= 0)
            return;

        try
        {
            _isBusy = true;
            SetLoading(true);
            ShowMessage(null);

            var role = await _service.GetByIdAsync(_roleId);

            if (role == null)
            {
                ShowMessage("تعذر العثور على بيانات الدور.");
                return;
            }

            NameEntry.Text = role.Name;
            CodeEntry.Text = role.Code;
            DescriptionEditor.Text = role.Description;
            IsSystemRoleSwitch.IsToggled = role.IsSystemRole;
            IsActiveSwitch.IsToggled = role.IsActive;

            _isLoaded = true;
        }
        catch (Exception ex)
        {
            ShowMessage($"تعذر تحميل الدور: {ex.Message}");
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

        var name = NameEntry.Text?.Trim() ?? string.Empty;
        var code = CodeEntry.Text?.Trim() ?? string.Empty;
        var description = DescriptionEditor.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            ShowMessage("يرجى إدخال اسم الدور.");
            return;
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            ShowMessage("يرجى إدخال كود الدور.");
            return;
        }

        try
        {
            _isBusy = true;
            SetLoading(true);
            ShowMessage(null);

            var request = new UpdateAdminRoleRequest
            {
                Name = name,
                Code = code,
                Description = description,
                IsSystemRole = IsSystemRoleSwitch.IsToggled,
                IsActive = IsActiveSwitch.IsToggled
            };

            var ok = await _service.UpdateAsync(_roleId, request);

            if (!ok)
            {
                ShowMessage("فشل تحديث الدور.");
                return;
            }

            await DisplayAlert("نجاح", "تم تحديث الدور بنجاح.", "موافق");
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

        NameEntry.IsEnabled = !isLoading;
        CodeEntry.IsEnabled = !isLoading;
        DescriptionEditor.IsEnabled = !isLoading;
        IsSystemRoleSwitch.IsEnabled = !isLoading;
        IsActiveSwitch.IsEnabled = !isLoading;
    }

    private void ShowMessage(string? message)
    {
        MessageLabel.Text = message ?? string.Empty;
        MessageLabel.IsVisible = !string.IsNullOrWhiteSpace(message);
    }
}