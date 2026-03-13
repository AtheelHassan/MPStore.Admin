using Microsoft.Extensions.Logging;
using MPStore.Admin.Helpers;
using MPStore.Admin.Services;

namespace MPStore.Admin
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton(new HttpClient
            {
                BaseAddress = new Uri(ApiConfig.BaseUrl)
            });

            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<StoresService>();
            builder.Services.AddSingleton<StoreUsersService>();
            builder.Services.AddSingleton<StoreRolesService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}