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
    /// A signal nobody could take — no screen registered, or the driver is capturing a
    /// signature — is kept and offered again at the next registration. It is never dropped
    /// silently: the whole point is that the driver stops driving to a trip that is gone.
    /// </para>
    /// </remarks>
    public class RouteSignalCoordinator
    {
        private readonly INotificationApiService _api;

        private readonly List<NotificationDto> _pending = new();
        private readonly SemaphoreSlim _gate = new(1, 1);

        private IRouteSignalHandler? _handler;

        public RouteSignalCoordinator(INotificationApiService api)
        {
            _api = api;
        }

        /// <summary>Called by a screen as it comes to the front.</summary>
        public void Register(IRouteSignalHandler handler)
        {
            _handler = handler;

            // Whatever arrived while nobody could take it gets its chance now, after a beat.
            // Registration happens in OnAppearing, and a popup raised at that instant can
            // land on the page the driver is leaving rather than the one arriving.
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
        public async Task ReceiveAsync(NotificationDto signal)
        {
            if (signal is null || !signal.IsSignal) return;

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
                RouteSignalOutcome outcome;

                try
                {
                    outcome = await handler.HandleRouteSignalAsync(signal);
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
        /// Takes a signal off the pending list and deletes it on the server.
        /// </summary>
        /// <remarks>
        /// It is removed locally whether or not the delete lands. A signal that stays on the
        /// server because the network dropped expires within the hour on its own; one that
        /// stays in this list would interrupt the driver again on the next screen for a change
        /// they have already been told about.
        /// </remarks>
        private async Task ConsumeAsync(NotificationDto signal)
        {
            await _gate.WaitAsync();

            try
            {
                _pending.RemoveAll(x => x.Id == signal.Id);
            }
            finally
            {
                _gate.Release();
            }

            if (signal.RecipientRecordId is not { } recordId)
                return;

            try
            {
                await _api.DeleteSignalAsync(recordId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RouteSignalCoordinator: could not delete signal. {ex.Message}");
            }
        }
    }
}
