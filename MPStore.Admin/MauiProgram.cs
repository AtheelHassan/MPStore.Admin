using Microsoft.Extensions.Logging;
using MPStore.Admin.Helpers;
using MPStore.Admin.Services;
using MPStore.Admin.Views;

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

            builder.Services.AddSingleton<AppShell>();

            builder.Services.AddTransient<AuthHeaderHandler>();

            builder.Services.AddHttpClient("ApiClient", client =>
            {
                client.BaseAddress = new Uri(ApiConfig.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            builder.Services.AddTransient<AuthService>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                return new AuthService(factory.CreateClient("ApiClient"));
            });

            builder.Services.AddHttpClient<StoresService>(client =>
            {
                client.BaseAddress = new Uri(ApiConfig.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<StoreUsersService>(client =>
            {
                client.BaseAddress = new Uri(ApiConfig.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<StoreRolesService>(client =>
            {
                client.BaseAddress = new Uri(ApiConfig.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<AdminUsersService>(client =>
            {
                client.BaseAddress = new Uri(ApiConfig.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<AdminRolesService>(client =>
            {
                client.BaseAddress = new Uri(ApiConfig.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddTransient<Login>();
            builder.Services.AddTransient<StoresList>();
            builder.Services.AddTransient<AddStore>();
            builder.Services.AddTransient<EditStore>();
            builder.Services.AddTransient<StoreUsersList>();
            builder.Services.AddTransient<AddStoreUser>();
            builder.Services.AddTransient<EditStoreUser>();
            builder.Services.AddTransient<AdminUsersList>();
            builder.Services.AddTransient<AdminRolesList>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}