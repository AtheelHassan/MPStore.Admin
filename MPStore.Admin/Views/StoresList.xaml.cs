using System.Collections.ObjectModel;
using System.Windows.Input;
using MPStore.Admin.Services;

namespace MPStore.Admin.Views
{
    public partial class StoresList : ContentPage
    {
        private readonly StoresService _storesService;
        private bool _isBusy;

        public ObservableCollection<StoreItemViewModel> Stores { get; } = new();

        public ICommand EditCommand { get; }
        public ICommand ViewCommand { get; }

        public StoresList(StoresService storesService)
        {
            InitializeComponent();

            _storesService = storesService;

            EditCommand = new Command<StoreItemViewModel>(async item => await OnEditStore(item));
            ViewCommand = new Command<StoreItemViewModel>(async item => await OnViewStore(item));

            BindingContext = this;
            StoresCollectionView.ItemsSource = Stores;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (Stores.Count == 0)
                await LoadStoresAsync();
        }

        private async Task LoadStoresAsync()
        {
            if (_isBusy)
                return;

            try
            {
                SetBusy(true);
                HideMessage();

                var result = await _storesService.GetStoresAsync();

                Stores.Clear();

                if (result == null || result.Count == 0)
                    return;

                foreach (var store in result)
                {
                    Stores.Add(new StoreItemViewModel
                    {
                        StoreId = store.Id,
                        Name = store.Name,
                        Slug = store.Slug,
                        Description = string.IsNullOrWhiteSpace(store.Description) ? "لا يوجد وصف" : store.Description,
                        IsActive = store.IsActive,
                        IsActiveText = store.IsActive ? "نشط" : "متوقف",
                        CreatedAt = store.CreatedAtUtc,
                        CreatedAtText = store.CreatedAtUtc == default
                            ? "تاريخ غير متوفر"
                            : $"تاريخ الإنشاء: {store.CreatedAtUtc:yyyy/MM/dd}"
                    });
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"تعذر تحميل المتاجر: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadStoresAsync();
        }

        private async void OnAddStoreClicked(object sender, EventArgs e)
        {
            await DisplayAlert("إضافة متجر", "سيتم ربط صفحة إضافة متجر لاحقًا.", "موافق");
        }

        private async Task OnEditStore(StoreItemViewModel? item)
        {
            if (item == null)
                return;

            await DisplayAlert("تعديل متجر", $"سيتم فتح تعديل المتجر: {item.Name}", "موافق");
        }

        private async Task OnViewStore(StoreItemViewModel? item)
        {
            if (item == null)
                return;

            var info =
                $"اسم المتجر: {item.Name}\n" +
                $"Slug: {item.Slug}\n" +
                $"الحالة: {item.IsActiveText}\n" +
                $"الوصف: {item.Description}";

            await DisplayAlert("تفاصيل المتجر", info, "موافق");
        }

        private void SetBusy(bool value)
        {
            _isBusy = value;

            LoadingIndicator.IsVisible = value;
            LoadingIndicator.IsRunning = value;
        }

        private void ShowMessage(string message)
        {
            MessageLabel.Text = message;
            MessageLabel.IsVisible = true;
        }

        private void HideMessage()
        {
            MessageLabel.Text = string.Empty;
            MessageLabel.IsVisible = false;
        }

        public class StoreItemViewModel
        {
            public long StoreId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Slug { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public string IsActiveText { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public string CreatedAtText { get; set; } = string.Empty;
        }
    }
}