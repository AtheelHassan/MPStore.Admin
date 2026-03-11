using MPStore.Admin.Views;

namespace MPStore.Admin
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(Views.StoresList), typeof(Views.StoresList));
            Routing.RegisterRoute(nameof(Views.AddStore), typeof(Views.AddStore));
            Routing.RegisterRoute(nameof(Views.EditStore), typeof(Views.EditStore));

            Routing.RegisterRoute(nameof(StoreUsersList), typeof(StoreUsersList));

            Routing.RegisterRoute(nameof(AddStoreUser), typeof(AddStoreUser));

            Routing.RegisterRoute(nameof(EditStoreUser), typeof(EditStoreUser));
        }
    }
}