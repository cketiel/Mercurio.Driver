using Raphael.Driver.Services;
using System.ComponentModel;
using System.Diagnostics;

namespace Raphael.Driver.Controls
{
    /// <summary>
    /// Centred page title, with an optional notification bell and unread counter.
    /// </summary>
    /// <remarks>
    /// Used as <c>Shell.TitleView</c>. Android left-aligns the Shell title and gives no way to
    /// centre it, so the title area is replaced wholesale.
    ///
    /// <para>
    /// The bell reads the count straight from the singleton <see cref="NotificationStore"/>
    /// rather than keeping one of its own, so the badge and the notifications page cannot end
    /// up disagreeing.
    /// </para>
    /// </remarks>
    public partial class PageTitleBar : ContentView
    {
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(
                nameof(Title),
                typeof(string),
                typeof(PageTitleBar),
                string.Empty);

        public static readonly BindableProperty ShowBellProperty =
            BindableProperty.Create(
                nameof(ShowBell),
                typeof(bool),
                typeof(PageTitleBar),
                true,
                propertyChanged: OnShowBellChanged);

        /// <summary>Text shown centred in the navigation bar.</summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Whether the bell is shown. Off for pages the driver reaches from somewhere else:
        /// a bell on a detail page competes with the back arrow for the same glance.
        /// </summary>
        public bool ShowBell
        {
            get => (bool)GetValue(ShowBellProperty);
            set => SetValue(ShowBellProperty, value);
        }

        private NotificationStore? _store;

        public PageTitleBar()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private static void OnShowBellChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is PageTitleBar bar)
                bar.BellArea.IsVisible = (bool)newValue;
        }

        private void OnLoaded(object? sender, EventArgs e)
        {
            BellArea.IsVisible = ShowBell;

            if (!ShowBell)
                return;

            _store = ServiceHelper.GetService<NotificationStore>();

            if (_store is null)
                return;

            _store.PropertyChanged += OnStoreChanged;

            UpdateBadge();
        }

        private void OnUnloaded(object? sender, EventArgs e)
        {
            // The store outlives the page. Without this the singleton keeps a reference to
            // every title bar that was ever shown.
            if (_store is not null)
                _store.PropertyChanged -= OnStoreChanged;

            _store = null;
        }

        private void OnStoreChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NotificationStore.UnreadCount))
                UpdateBadge();
        }

        private void UpdateBadge()
        {
            var count = _store?.UnreadCount ?? 0;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                BadgeBorder.IsVisible = count > 0;

                // Past ninety-nine the exact number stops telling the driver anything they
                // did not already know.
                BadgeLabel.Text = count > 99 ? "99+" : count.ToString();
            });
        }

        private async void OnBellTapped(object? sender, TappedEventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//NotificationsPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PageTitleBar: could not navigate. {ex.Message}");
            }
        }
    }
}
