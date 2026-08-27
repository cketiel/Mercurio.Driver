using CommunityToolkit.Mvvm.ComponentModel;
using Raphael.Driver.DTOs;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Raphael.Driver.Services
{
    /// <summary>
    /// The one place that knows what the driver has in their inbox.
    /// </summary>
    /// <remarks>
    /// The bell and the notifications page read from here, so they cannot end up showing
    /// different counts. Everything that changes the list — a refresh, a live message from the
    /// hub, a read mark, a hide — goes through this object.
    /// </remarks>
    public partial class NotificationStore : ObservableObject
    {
        private readonly INotificationApiService _api;
        private readonly HiddenNotificationStore _hidden;

        /// <summary>Everything the server returned, hidden ones included.</summary>
        private readonly List<NotificationDto> _all = new();

        /// <summary>What the driver actually sees, newest first.</summary>
        public ObservableCollection<NotificationDto> Items { get; } = new();

        [ObservableProperty]
        private int _unreadCount;

        [ObservableProperty]
        private bool _isBusy;

        /// <summary>
        /// True when the last refresh did not come back. Distinct from an empty inbox.
        /// </summary>
        /// <remarks>
        /// Telling a driver "no notifications" when the server actually refused the call means
        /// a deployment with <c>DriverRoleIds</c> wrong looks exactly like a quiet shift.
        /// </remarks>
        [ObservableProperty]
        private bool _loadFailed;

        public NotificationStore(
            INotificationApiService api,
            HiddenNotificationStore hidden)
        {
            _api = api;
            _hidden = hidden;
        }

        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            if (IsBusy) return;

            IsBusy = true;

            try
            {
                var fromServer = await _api.GetAsync(cancellationToken);

                if (fromServer is null)
                {
                    // The call did not come back. Keep whatever the driver already had on
                    // screen — wiping a cancellation off their list because the network
                    // dropped is worse than showing one that may be stale.
                    LoadFailed = true;
                    return;
                }

                LoadFailed = false;

                // Prune first: an identifier the server no longer returns has expired, and
                // keeping it in the hidden list would make that list grow for ever.
                _hidden.PruneTo(fromServer.Select(n => n.Id));

                _all.Clear();
                _all.AddRange(fromServer);

                Rebuild();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NotificationStore.RefreshAsync: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// A notification that just arrived over the hub, with the app open.
        /// </summary>
        public void Receive(NotificationDto notification)
        {
            if (notification is null) return;

            // Signals belong to the route coordinator, not to the inbox. The server already
            // keeps them out of the list endpoints; over the hub everything arrives down the
            // same wire, so this is where that separation is kept.
            if (notification.IsSignal) return;

            var existing = _all.FindIndex(n => n.Id == notification.Id);

            if (existing >= 0)
                _all[existing] = notification;
            else
                _all.Add(notification);

            Rebuild();
        }

        public async Task<bool> MarkViewedAsync(NotificationDto notification)
        {
            if (notification?.RecipientRecordId is not { } recordId) return false;

            if (!notification.IsUnread) return true;

            var ok = await _api.MarkViewedAsync(recordId);

            if (!ok) return false;

            if (notification.MyRecipient is { } recipient)
                recipient.ViewedAtUtc = DateTime.UtcNow;

            Rebuild();

            return true;
        }

        public async Task<bool> MarkUnviewedAsync(NotificationDto notification)
        {
            if (notification?.RecipientRecordId is not { } recordId) return false;

            var ok = await _api.MarkUnviewedAsync(recordId);

            if (!ok) return false;

            if (notification.MyRecipient is { } recipient)
                recipient.ViewedAtUtc = null;

            Rebuild();

            return true;
        }

        public async Task<bool> MarkAllViewedAsync()
        {
            var ok = await _api.MarkAllViewedAsync();

            if (!ok) return false;

            foreach (var recipient in _all.Select(n => n.MyRecipient))
            {
                if (recipient is not null)
                    recipient.ViewedAtUtc ??= DateTime.UtcNow;
            }

            Rebuild();

            return true;
        }

        /// <summary>
        /// Takes a notification off this phone's list. Never leaves the device.
        /// </summary>
        public void Hide(NotificationDto notification)
        {
            if (notification is null) return;

            _hidden.Hide(notification.Id);

            Rebuild();
        }

        /// <summary>Everything the driver had, gone. Called on sign out.</summary>
        public void Clear()
        {
            _all.Clear();
            _hidden.Clear();

            Rebuild();
        }

        /// <summary>
        /// Rebuilds the visible list and the badge from one source, on the UI thread.
        /// </summary>
        private void Rebuild()
        {
            void Apply()
            {
                var visible = _all
                    .Where(n => !_hidden.IsHidden(n.Id))
                    .OrderByDescending(n => n.CreatedAtUtc)
                    .ToList();

                Items.Clear();

                foreach (var notification in visible)
                    Items.Add(notification);

                // The badge counts what is on screen. Counting hidden rows would put a number
                // over a list that does not contain them.
                UnreadCount = visible.Count(n => n.IsUnread);
            }

            if (MainThread.IsMainThread)
                Apply();
            else
                MainThread.BeginInvokeOnMainThread(Apply);
        }
    }
}
