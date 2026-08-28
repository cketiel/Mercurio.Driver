using System.Diagnostics;
using System.Text.Json;

namespace Raphael.Driver.Services
{
    /// <summary>
    /// Route signals this phone has already acted on, kept on this phone only.
    /// </summary>
    /// <remarks>
    /// The app used to delete a signal on the server the moment it acted on it. It cannot any
    /// more: a signal is shown in the driver's bell now, and deleting it would take a row off
    /// a list the driver had not read yet. So "already acted on" becomes a device decision,
    /// exactly like <see cref="HiddenNotificationStore"/>, and the row itself ages out under
    /// the retention policy.
    ///
    /// <para>
    /// Without this the driver would be interrupted twice by the same route change: once when
    /// it arrived over the hub, and again the next time the app signed in or its socket came
    /// back and drained the signals it had missed.
    /// </para>
    ///
    /// <para>
    /// ⚠️ The key is scoped to the driver's UserId. Phones are handed over between shifts, and
    /// a shared list would swallow the new driver's first route change.
    /// </para>
    ///
    /// <para>
    /// Pruned on every sync against what the server returned. Signals live twelve hours, so it
    /// empties itself within a shift and never grows.
    /// </para>
    /// </remarks>
    public class ConsumedSignalStore
    {
        private const string KeyPrefix = "ConsumedSignals";

        private readonly HashSet<Guid> _consumed = new();

        private string _key = string.Empty;

        /// <summary>
        /// Loads the list belonging to whoever is signed in now. Called after every sign in.
        /// </summary>
        public void LoadForCurrentUser()
        {
            var userId = Preferences.Get("UserId", string.Empty);

            _consumed.Clear();

            if (string.IsNullOrWhiteSpace(userId))
            {
                _key = string.Empty;
                return;
            }

            _key = $"{KeyPrefix}:{userId}";

            try
            {
                var raw = Preferences.Get(_key, string.Empty);

                if (string.IsNullOrWhiteSpace(raw))
                    return;

                var ids = JsonSerializer.Deserialize<List<Guid>>(raw);

                if (ids is not null)
                {
                    foreach (var id in ids)
                        _consumed.Add(id);
                }
            }
            catch (Exception ex)
            {
                // A corrupt list is not worth a crash. Worst case the driver is told once more
                // that their route changed, which is the safe direction.
                Debug.WriteLine($"ConsumedSignalStore: could not read {_key}. {ex.Message}");
            }
        }

        public bool WasConsumed(Guid notificationId)
            => _consumed.Contains(notificationId);

        public void MarkConsumed(Guid notificationId)
        {
            if (_consumed.Add(notificationId))
                Persist();
        }

        /// <summary>
        /// Drops identifiers the server no longer returns, so the list cannot grow forever.
        /// </summary>
        public void PruneTo(IEnumerable<Guid> stillOnServer)
        {
            var alive = new HashSet<Guid>(stillOnServer);

            if (_consumed.RemoveWhere(id => !alive.Contains(id)) > 0)
                Persist();
        }

        /// <summary>Forgets everything held in memory. Called on sign out.</summary>
        public void Clear()
        {
            _consumed.Clear();
            _key = string.Empty;
        }

        private void Persist()
        {
            if (string.IsNullOrEmpty(_key))
                return;

            try
            {
                Preferences.Set(_key, JsonSerializer.Serialize(_consumed.ToList()));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ConsumedSignalStore: could not write {_key}. {ex.Message}");
            }
        }
    }
}
