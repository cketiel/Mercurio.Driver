﻿using Mercurio.Driver.Models;
using Mercurio.Driver.Resources.Styles;
using Microsoft.Extensions.Configuration;


namespace Mercurio.Driver
{
    public partial class App : Application
    {
        private readonly DarkTheme _darkThemeDictionary;
        public App()
        {
            //AppConfig.Init();
            InitializeComponent();
            Preferences.Set("ApiBaseUrl", "https://krasnovbw-001-site1.rtempurl.com/"); 
            //Preferences.Set("ApiBaseUrl", "https://localhost:7244/");
            MainPage = new AppShell();
            // Go directly to LoginPage on startup
            //Shell.Current.GoToAsync("//LoginPage");

            _darkThemeDictionary = new DarkTheme();

            // Suscribirse al evento de cambio de tema del sistema operativo.
            Current.RequestedThemeChanged += OnRequestedThemeChanged;

            // Establecer el tema correcto cuando la aplicación se inicia por primera vez.
            LoadTheme(Current.RequestedTheme);

        }

        /// <summary>
        /// This method is called by the framework AFTER the app has been initialized
        /// and MainPage has been assigned. Shell.Current is GUARANTEED not to be null here.
        /// </summary>
        protected override void OnStart()
        {           
            Shell.Current.GoToAsync("///LoginPage");
        }

        private void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadTheme(e.RequestedTheme);
            });
        }

        private void LoadTheme(AppTheme theme)
        {
            var mergedDictionaries = Current.Resources.MergedDictionaries;

            if (mergedDictionaries.Contains(_darkThemeDictionary))
            {
                mergedDictionaries.Remove(_darkThemeDictionary);
            }

            if (theme == AppTheme.Dark)
            {
                // Como _darkThemeDictionary ya es una instancia completa, simplemente la añadimos.
                mergedDictionaries.Add(_darkThemeDictionary);
            }
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
