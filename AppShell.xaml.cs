using Mercurio.Driver.Views;

namespace Mercurio.Driver
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(SchedulePage), typeof(SchedulePage));
            Routing.RegisterRoute(nameof(TodaySchedulePage), typeof(TodaySchedulePage));

            //Routing.RegisterRoute("LoginPage", typeof(Views.LoginPage));
            //Routing.RegisterRoute("HomePage", typeof(Views.HomePage));
        }
    }
}
