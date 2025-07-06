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

            // --- INYECCIÓN DE DEPENDENCIAS ---

            // Servicios (Singleton porque no guardan estado y pueden ser compartidos)
            builder.Services.AddSingleton<IScheduleService, ScheduleService>();

            // ViewModels (Transient porque cada página debería tener su propia instancia)
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<ScheduleViewModel>();
            builder.Services.AddTransient<TodayScheduleViewModel>();

            // Vistas / Páginas (Transient)
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SchedulePage>();
            builder.Services.AddTransient<TodaySchedulePage>();



#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
