 using CommunityToolkit.Maui;
using Mercurio.Driver.Models;
using Mercurio.Driver.Services;
using Mercurio.Driver.ViewModels;
using Mercurio.Driver.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Mercurio.Driver
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {            
            var builder = MauiApp.CreateBuilder();
          
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("fa-solid-900.ttf", "FontAwesomeSolid");
                });

            // --- DEPENDENCY INJECTION ---

            // Services (Singleton because they do not save state and can be shared)
            builder.Services.AddSingleton<IScheduleService, ScheduleService>();
            builder.Services.AddSingleton<IGpsService, GpsService>();
            builder.Services.AddSingleton<IMapService, MapService>();
            builder.Services.AddSingleton<ISessionManagerService, SessionManagerService>();
            builder.Services.AddSingleton<App>();
            builder.Services.AddSingleton<IRunService, RunService>();

            builder.Services.AddSingleton<IPhoneDialer>(PhoneDialer.Default);

            // ViewModels (Transient because each page should have its own instance)
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<ScheduleViewModel>();
            builder.Services.AddTransient<TodayScheduleViewModel>();
            builder.Services.AddTransient<PullOutDetailPage>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<EventDetailPageViewModel>();
            builder.Services.AddTransient<SignatureViewModel>();
            builder.Services.AddTransient<FutureScheduleViewModel>();
            builder.Services.AddTransient<FutureDetailViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<HistoryViewModel>();

            // Views/Pages (Transient)
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SchedulePage>();
            builder.Services.AddTransient<TodaySchedulePage>();
            builder.Services.AddTransient<PullOutDetailPageViewModel>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<EventDetailPage>();
            builder.Services.AddTransient<SignaturePage>();
            builder.Services.AddTransient<FutureSchedulePage>();
            builder.Services.AddTransient<FutureDetailPage>();
            builder.Services.AddSingleton<DashboardPage>();
            builder.Services.AddTransient<HistoryPage>();

            // We register the GPS service using a factory
            // which gets the static instance of MainActivity
            builder.Services.AddSingleton<IGpsService>(provider =>
            {
#if ANDROID
                
                return MainActivity.GpsService ?? new Mercurio.Driver.Services.GpsServiceAndroid();
#elif IOS
    return new Mercurio.Driver.Services.GpsServiceIos();
#else
    // Implementación por defecto para otras plataformas (Windows, etc.)
    return new GpsService(); 
#endif
            });


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
