using Mercurio.Driver.ViewModels;
using Mercurio.Driver.Views;

namespace Mercurio.Driver
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            BindingContext = new AppShellViewModel();

            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(SchedulePage), typeof(SchedulePage));
            Routing.RegisterRoute(nameof(TodaySchedulePage), typeof(TodaySchedulePage));
            Routing.RegisterRoute(nameof(Views.PullOutDetailPage), typeof(Views.PullOutDetailPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(EventDetailPage), typeof(EventDetailPage));
            Routing.RegisterRoute(nameof(SignaturePage), typeof(SignaturePage));

            //Routing.RegisterRoute("LoginPage", typeof(Views.LoginPage));
            //Routing.RegisterRoute("HomePage", typeof(Views.HomePage));

            Loaded += async (sender, args) =>
            {
                await GoToLogin();
            };
        }

        private async Task GoToLogin()
        {
            // We use "///" to make LoginPage display as a modal page,
            // without the side menu (Flyout). This resets the navigation stack.
            // The /// prefix is ​​very important. It tells MAUI Shell: "Forget the current navigation, and make this page (LoginPage) the only one on the screen."
            // This has the effect of hiding the hamburger menu and navigation bar from the Shell
            await Current.GoToAsync($"///{nameof(LoginPage)}");
        }
    }
}
