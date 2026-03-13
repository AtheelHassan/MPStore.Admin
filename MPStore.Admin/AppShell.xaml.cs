using MPStore.Admin.Views;

namespace MPStore.Admin
{
    public partial class AppShell : Shell
    {
        public const string RouteStoresList = "stores-list";
        public const string RouteAddStore = "add-store";
        public const string RouteEditStore = "edit-store";

        public const string RouteStoreUsersList = "store-users-list";
        public const string RouteAddStoreUser = "add-store-user";
        public const string RouteEditStoreUser = "edit-store-user";

        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(RouteStoresList, typeof(StoresList));
            Routing.RegisterRoute(RouteAddStore, typeof(AddStore));
            Routing.RegisterRoute(RouteEditStore, typeof(EditStore));

            Routing.RegisterRoute(RouteStoreUsersList, typeof(StoreUsersList));
            Routing.RegisterRoute(RouteAddStoreUser, typeof(AddStoreUser));
            Routing.RegisterRoute(RouteEditStoreUser, typeof(EditStoreUser));
        }
    }
}