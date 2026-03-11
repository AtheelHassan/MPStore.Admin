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
        }
    }
}