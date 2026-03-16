using MPStore.Admin.Views;

namespace MPStore.Admin
{
    public partial class AppShell : Shell
    {
        public const string RouteDashboard = "dashboard";

        public const string RouteStoresList = "stores-list";
        public const string RouteAddStore = "add-store";
        public const string RouteAddStoreDetails = "add-store-details";
        public const string RouteEditStore = "edit-store";

        public const string RouteStoreUsersList = "store-users-list";
        public const string RouteAddStoreUser = "add-store-user";
        public const string RouteEditStoreUser = "edit-store-user";

        public const string RouteAdminUsersList = "admin-users-list";
        public const string RouteAddAdminUser = "add-admin-user";
        public const string RouteEditAdminUser = "edit-admin-user";

        public const string RouteAdminRolesList = "admin-roles-list";
        public const string RouteAddAdminRole = "add-admin-role";
        public const string RouteEditAdminRole = "edit-admin-role";
        public const string RouteAdminRolePermissions = "admin-role-permissions";

        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(RouteDashboard, typeof(Dashboard));

            Routing.RegisterRoute(RouteStoresList, typeof(StoresList));
            Routing.RegisterRoute(RouteAddStore, typeof(AddStore));
            Routing.RegisterRoute(RouteAddStoreDetails, typeof(AddStoreDetails));
            Routing.RegisterRoute(RouteEditStore, typeof(EditStore));

            Routing.RegisterRoute(RouteStoreUsersList, typeof(StoreUsersList));
            Routing.RegisterRoute(RouteAddStoreUser, typeof(AddStoreUser));
            Routing.RegisterRoute(RouteEditStoreUser, typeof(EditStoreUser));

            Routing.RegisterRoute(RouteAdminUsersList, typeof(AdminUsersList));
            Routing.RegisterRoute(RouteAddAdminUser, typeof(AddAdminUser));
            Routing.RegisterRoute(RouteEditAdminUser, typeof(EditAdminUser));

            Routing.RegisterRoute(RouteAdminRolesList, typeof(AdminRolesList));
            Routing.RegisterRoute(RouteAddAdminRole, typeof(AddAdminRole));
            Routing.RegisterRoute(RouteEditAdminRole, typeof(EditAdminRole));
            Routing.RegisterRoute(RouteAdminRolePermissions, typeof(AdminRolePermissions));
        }
    }
}