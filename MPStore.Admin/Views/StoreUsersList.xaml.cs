using System.Collections.ObjectModel;
using MPStore.Admin.Models.StoreUsers;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views;

[QueryProperty(nameof(StoreId), "storeId")]
public partial class StoreUsersList : ContentPage
{
    private readonly StoreUsersService _service;
    private long _storeId;
    private bool _isBusy;

    public ObservableCollection<StoreUserDto> Users { get; set; } = new();

    public string StoreId
    {
        get => _storeId.ToString();
        set
        {
            if (long.TryParse(value, out var id))
                _storeId = id;
        }
    }

    public StoreUsersList(StoreUsersService service)
    {
        InitializeComponent();
        _service = service;
        BindingContext = this;
        UsersCollectionView.ItemsSource = Users;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUsers();
    }

    private async Task LoadUsers()
    {
        if (_isBusy || _storeId <= 0)
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
        await Shell.Current.GoToAsync($"{nameof(AddStoreUser)}?storeId={_storeId}");
    }

    private async void OnEdit(object sender, EventArgs e)
    {
        var btn = sender as Button;
        var user = btn?.BindingContext as StoreUserDto;

        if (user == null)
            return;

        await Shell.Current.GoToAsync($"EditStoreUser?userId={user.Id}&storeId={_storeId}");
    }

    private async void OnDelete(object sender, EventArgs e)
    {
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
        await Shell.Current.GoToAsync($"{nameof(EditStore)}?storeId={_storeId}");
    }
   

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadUsers();
    }
}