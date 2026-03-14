using MPStore.Admin.Models.AdminRoles;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views;

public partial class AddAdminRole : ContentPage
{
    private readonly AdminRolesService _service;
    private bool _isBusy;

    public AddAdminRole(AdminRolesService service)
    {
        InitializeComponent();
        _service = service;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(AppShell.RouteAdminRolesList);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_isBusy)
            return;

        var name = NameEntry.Text?.Trim();
        var code = CodeEntry.Text?.Trim();
        var description = DescriptionEditor.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("تنبيه", "يرجى إدخال اسم الدور", "حسناً");
            return;
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            await DisplayAlert("تنبيه", "يرجى إدخال كود الدور", "حسناً");
            return;
        }

        try
        {
            _isBusy = true;

            var request = new CreateAdminRoleRequest
            {
                Name = name,
                Code = code,
                Description = description,
                IsSystemRole = IsSystemRoleSwitch.IsToggled,
                IsActive = IsActiveSwitch.IsToggled
            };

            var ok = await _service.CreateAsync(request);

            if (!ok)
            {
                await DisplayAlert("خطأ", "فشل إنشاء الدور", "موافق");
                return;
            }

            await DisplayAlert("نجاح", "تم إنشاء الدور بنجاح", "موافق");

            await Shell.Current.GoToAsync(AppShell.RouteAdminRolesList);
        }
        catch (Exception ex)
        {
            await DisplayAlert("خطأ", ex.Message, "موافق");
        }
        finally
        {
            _isBusy = false;
        }
    }
}