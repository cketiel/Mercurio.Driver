using Raphael.Driver.ViewModels;
using Raphael.Driver.Views;
using Raphael.Driver.Helpers;

namespace Raphael.Driver
{
    public partial class AppShell : Shell
    {
        /// <summary>
        /// Gets the current application version displayed in the Shell.
        /// </summary>
        public string Version => AppVersion.Display;

        public AppShell()
        {
            InitializeComponent();

            BindingContext = new AppShellViewModel();
            //BindingContext = Handler.MauiContext.Services.GetService<AppShellViewModel>()

            // Set the Shell title including the current application version.
            Title = $"Raphael Driver {Version}";

            //Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            //Routing.RegisterRoute(nameof(SchedulePage), typeof(SchedulePage));
            Routing.RegisterRoute(nameof(TodaySchedulePage), typeof(TodaySchedulePage));
            Routing.RegisterRoute(nameof(Views.PullOutDetailPage), typeof(Views.PullOutDetailPage));
            //Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(EventDetailPage), typeof(EventDetailPage));
            Routing.RegisterRoute(nameof(SignaturePage), typeof(SignaturePage));

            Routing.RegisterRoute(nameof(FutureSchedulePage), typeof(FutureSchedulePage));
            Routing.RegisterRoute(nameof(FutureDetailPage), typeof(FutureDetailPage));
            Routing.RegisterRoute(nameof(HistoryPage), typeof(HistoryPage));
            Routing.RegisterRoute(nameof(ContactPage), typeof(ContactPage));

            //Routing.RegisterRoute("LoginPage", typeof(Views.LoginPage));
            //Routing.RegisterRoute("HomePage", typeof(Views.HomePage));

        }

    }
}