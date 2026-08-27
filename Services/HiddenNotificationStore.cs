using System.Diagnostics;
using System.Text.Json;

namespace Raphael.Driver.Services
{
    /// <summary>
    /// Notifications this driver chose not to see, kept on this phone only.
    /// </summary>
    /// <remarks>
    /// Hiding is deliberately a device decision and never reaches the server. The rows on the
    /// server age out and are removed by the retention policy, which is the only thing allowed
    /// to delete a notification: a driver tidying their screen must not destroy the record of
    /// a cancellation somebody may have to answer for later.
    ///
    /// <para>
    /// ⚠️ The key is scoped to the driver's UserId. Phones are handed over between shifts, and
    /// a shared list would hide from the next driver the notices the previous one dismissed —
    /// their own trips, invisible with no explanation.
    /// </para>
    ///
    /// <para>
    /// The list is pruned on every refresh against what the server returned. Driver
    /// notifications live twelve hours, so it empties itself within a shift and never grows.
    /// </para>
    /// </remarks>
    public class HiddenNotificationStore
    {
        private const string KeyPrefix = "HiddenNotifications";

        private readonly HashSet<Guid> _hidden = new();

        private string _key = string.Empty;

        /// <summary>
        /// Loads the list belonging to whoever is signed in now. Called after every sign in.
        /// </summary>
        public void LoadForCurrentUser()
        {
            var userId = Preferences.Get("UserId", string.Empty);

            _hidden.Clear();

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
                        _hidden.Add(id);
                }
            }
            catch (Exception ex)
            {
                // A corrupt list is not worth a crash. Worst case the driver sees again
                // something they had dismissed, which is the safe direction.
                Debug.WriteLine($"HiddenNotificationStore: could not read {_key}. {ex.Message}");
            }
        }

        public bool IsHidden(Guid notificationId)
            => _hidden.Contains(notificationId);

        public void Hide(Guid notificationId)
        {
            if (_hidden.Add(notificationId))
                Persist();
        }

        public void Restore(Guid notificationId)
        {
            if (_hidden.Remove(notificationId))
                Persist();
        }

        /// <summary>
        /// Drops identifiers the server no longer returns, so the list cannot grow forever.
        /// </summary>
        public void PruneTo(IEnumerable<Guid> stillOnServer)
        {
            var alive = new HashSet<Guid>(stillOnServer);

            if (_hidden.RemoveWhere(id => !alive.Contains(id)) > 0)
                Persist();
        }

        /// <summary>Forgets everything. Called on sign out.</summary>
        public void Clear()
        {
            _hidden.Clear();
            _key = string.Empty;
        }

        private void Persist()
        {
            if (string.IsNullOrEmpty(_key))
                return;

            try
            {
                Preferences.Set(_key, JsonSerializer.Serialize(_hidden.ToList()));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HiddenNotificationStore: could not write {_key}. {ex.Message}");
            }
        }
    }
}
