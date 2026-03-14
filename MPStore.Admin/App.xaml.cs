using Microsoft.Extensions.DependencyInjection;

namespace MPStore.Admin
{
    public partial class App : Application
    {
        public App()
        {

           // Preferences.Remove("AdminSession");
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}