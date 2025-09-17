using Mercurio.Driver.Models;
using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.Configuration;
using Mercurio.Driver.Models;

namespace Mercurio.Driver
{
    public partial class App : Application
    {
        public App()
        {
            //AppConfig.Init();
            InitializeComponent();
            Preferences.Set("ApiBaseUrl", "https://krasnovbw-001-site1.rtempurl.com/"); 
            //Preferences.Set("ApiBaseUrl", "https://localhost:7244/");
            MainPage = new AppShell();
            // Go directly to LoginPage on startup
            //Shell.Current.GoToAsync("//LoginPage");
        }

        /// <summary>
        /// This method is called by the framework AFTER the app has been initialized
        /// and MainPage has been assigned. Shell.Current is GUARANTEED not to be null here.
        /// </summary>
        protected override void OnStart()
        {           
            Shell.Current.GoToAsync("///LoginPage");
        }
    }

 

    /*public static class AppConfig
    {
        public static AppSettings Settings { get; private set; }

        public static void Init()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(FileSystem.AppDataDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Settings = config.Get<AppSettings>();
        }
    }*/
}
