using Raphael.Driver.DTOs;
using System.Diagnostics;

namespace Raphael.Driver.Services
{
    /// <summary>
    /// Routes a route-change signal to whatever screen the driver is looking at.
    /// </summary>
    /// <remarks>
    /// A signal says the schedule on screen is out of date. Only the screen on top knows
    /// whether that matters to what it is showing, so the decision lives there
    /// (<see cref="IRouteSignalHandler"/>) and this only decides who gets asked and what
    /// happens to the signal afterwards.
    ///
    /// <para>
    /// ⚠️ Only a screen that was <b>already open</b> when the signal arrived is interrupted.
    /// The countdown exists because the driver is looking at a schedule we know to be wrong;
    /// every screen loads fresh data as it opens, so a driver who walks into today's schedule
    /// a minute later is already looking at the truth and has nothing to reload.
    /// </para>
    ///
    /// <para>
    /// The one signal that survives to be offered again is one a screen was shown and could not
    /// take: the signature, where interrupting loses the patient's signature and with it the
    /// proof that the trip happened.
    /// </para>
    ///
    /// <para>
    /// ⚠️ Interrupting is not the same as informing. Every signal reaches the bell through
    /// <see cref="NotificationStore"/> and stays there until the driver reads it. This class
    /// only decides when to put a blocking countdown in front of somebody who may be driving,
    /// and that is reserved for the two screens that would otherwise keep showing data we
    /// already know to be false.
    /// </para>
    /// </remarks>
    public class RouteSignalCoordinator
    {
        /// <summary>
        /// How fresh a signal has to be for the app to interrupt the driver with it.
        /// </summary>
        /// <remarks>
        /// A signal is worth reading for twelve hours — it sits in the bell like any other
        /// notice — but it is only worth <b>acting on</b> while it is new. Past the first hour
        /// every screen has reloaded on its own, and a countdown over a change the app already
        /// has buys nothing. This is also what keeps a driver who signs in mid-shift from being
        /// walked through a morning's worth of route changes one popup at a time.
        /// </remarks>
        public static readonly TimeSpan ActOnSignalsNewerThan = TimeSpan.FromHours(1);

        private readonly ConsumedSignalStore _consumed;
        private readonly INotificationApiService _api;

        private readonly List<NotificationDto> _pending = new();
        private readonly SemaphoreSlim _gate = new(1, 1);

        private IRouteSignalHandler? _handler;

        public RouteSignalCoordinator(
            INotificationApiService api,
            ConsumedSignalStore consumed)
        {
            _api = api;
            _consumed = consumed;
        }

        /// <summary>Called by a screen as it comes to the front.</summary>
        /// <remarks>
        /// The drain here is for the deferred case and nothing else: a signal that a screen was
        /// offered and could not take, which today means the signature. A signal that arrived
        /// while no screen was registered was already dropped in
        /// <see cref="ReceiveAsync"/> — the screen the driver opens next loads fresh data on
        /// the way in, so telling them to reload what they just loaded is noise.
        ///
        /// <para>
        /// After a beat, because registration happens in OnAppearing and a popup raised at that
        /// instant can land on the page the driver is leaving rather than the one arriving.
        /// </para>
        /// </remarks>
        public void Register(IRouteSignalHandler handler)
        {
            _handler = handler;

            _ = Task.Run(async () =>
            {
                await Task.Delay(400);
                await DrainPendingAsync();
            });
        }

        /// <summary>Called by a screen as it leaves.</summary>
        public void Unregister(IRouteSignalHandler handler)
        {
            if (ReferenceEquals(_handler, handler))
                _handler = null;
        }

        /// <summary>A signal that just arrived over the hub.</summary>
        /// <remarks>
        /// ⚠️ It is only worth showing to a screen that was <b>already open</b> when it arrived.
        /// The whole point of the countdown is that the driver is looking at a schedule we know
        /// to be wrong; every screen loads fresh data as it opens, so a driver who walks into
        /// today's schedule a minute later is already looking at the truth and has nothing to
        /// reload. The row is in the bell either way.
        /// </remarks>
        public async Task ReceiveAsync(NotificationDto signal)
        {
            if (signal is null || !signal.IsSignal) return;

            // Already dealt with on this phone. It stays in the bell, where the driver can
            // read it whenever they want; what it must not do is interrupt them twice.
            if (_consumed.WasConsumed(signal.Id)) return;

            // Nobody is looking at anything this could correct.
            if (_handler is null)
            {
                await ConsumeAsync(signal);
                return;
            }

            await _gate.WaitAsync();

            try
            {
                if (_pending.Any(x => x.Id == signal.Id))
                    return;

                _pending.Add(signal);
            }
            finally
            {
                _gate.Release();
            }

            await DrainPendingAsync();
        }

        /// <summary>
        /// Picks up signals that arrived while the app was closed or its socket was down.
        /// Called after sign in and whenever the live channel reconnects.
        /// </summary>
        public async Task SyncAsync()
        {
            try
            {
                var signals = await _api.GetSignalsAsync();

                // Prune first: an identifier the server no longer returns has expired, and
                // keeping it in the consumed list would make that list grow for ever.
                _consumed.PruneTo(signals.Select(x => x.Id));

                foreach (var signal in signals)
                    await ReceiveAsync(signal);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RouteSignalCoordinator.SyncAsync: {ex.Message}");
            }
        }

        /// <summary>Forgets everything. Called on sign out.</summary>
        public async Task ClearAsync()
        {
            await _gate.WaitAsync();

            try
            {
                _pending.Clear();
                _handler = null;
            }
            finally
            {
                _gate.Release();
            }

            _consumed.Clear();
        }

        private async Task DrainPendingAsync()
        {
            var handler = _handler;

            if (handler is null)
                return;

            List<NotificationDto> batch;

            await _gate.WaitAsync();

            try
            {
                if (_pending.Count == 0)
                    return;

                batch = _pending.ToList();
            }
            finally
            {
                _gate.Release();
            }

            foreach (var signal in batch)
            {
                // Old enough that every screen has reloaded on its own since it was written.
                // Taken off the list without a word: it is in the bell if the driver wants it.
                if (DateTime.UtcNow - signal.CreatedAtUtc > ActOnSignalsNewerThan)
                {
                    await ConsumeAsync(signal);
                    continue;
                }

                RouteSignalOutcome outcome;

                try
                {
                    // ⚠️ On the UI thread, always. A signal arrives on the hub's own thread,
                    // and the handlers put a popup on screen and navigate — Android refuses
                    // both from anywhere else. That exception used to be caught below, which
                    // left the driver with a list that refreshed itself and no explanation.
                    outcome = await MainThread.InvokeOnMainThreadAsync(
                        () => handler.HandleRouteSignalAsync(signal));
                }
                catch (Exception ex)
                {
                    // A screen that throws must not strand the signal for ever, but it must
                    // not consume it either: leave it pending for the next screen.
                    Debug.WriteLine($"RouteSignalCoordinator: handler threw. {ex.Message}");
                    continue;
                }

                if (outcome == RouteSignalOutcome.Deferred)
                    continue;

                await ConsumeAsync(signal);
            }
        }

        /// <summary>
        /// Takes a signal off the pending list and records that this phone acted on it.
        /// </summary>
        /// <remarks>
        /// It is no longer deleted on the server. A signal now shows in the driver's bell, and
        /// deleting it here would take a row off a list they have not read yet; it ages out on
        /// its own under the retention policy. What has to survive is the fact that this device
        /// already dealt with it, or the next sync would interrupt the driver again over a
        /// change they have already been shown.
        /// </remarks>
        private async Task ConsumeAsync(NotificationDto signal)
        {
            // Recorded before the pending list is touched: an app killed between the two would
            // rather leave a signal marked and unshown than show it twice at the wheel.
            _consumed.MarkConsumed(signal.Id);

            await _gate.WaitAsync();

            try
            {
                _pending.RemoveAll(x => x.Id == signal.Id);
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
