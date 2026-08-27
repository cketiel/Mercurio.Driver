using CommunityToolkit.Maui.Views;
using System.Diagnostics;

namespace Raphael.Driver.Controls
{
    /// <summary>
    /// Tells the driver the route changed, then resolves itself.
    /// </summary>
    /// <remarks>
    /// See the comment in the XAML for why it both blocks and closes on its own. In short:
    /// the driver must not be able to tap a list that is about to reorder, and must not be
    /// required to tap anything to get correct data.
    /// </remarks>
    public partial class RouteSignalPopup : Popup
    {
        /// <summary>
        /// Long enough to read two lines at the wheel, short enough that the schedule on
        /// screen is not wrong for meaningfully longer than it already was.
        /// </summary>
        public const int DefaultSeconds = 5;

        private readonly CancellationTokenSource _cancellation = new();

        private RouteSignalPopup(string title, string message, string actionLabel)
        {
            InitializeComponent();

            TitleLabel.Text = title;
            MessageLabel.Text = message;
            ActionButton.Text = actionLabel;
        }

        /// <summary>
        /// Shows it and returns when the countdown ends or the driver taps the button.
        /// </summary>
        public static async Task ShowAsync(
            string title,
            string message,
            string actionLabel,
            int seconds = DefaultSeconds)
        {
            var page = Shell.Current?.CurrentPage;

            if (page is null)
                return;

            var popup = new RouteSignalPopup(title, message, actionLabel);

            var countdown = popup.RunCountdownAsync(seconds);

            try
            {
                await page.ShowPopupAsync(popup);
            }
            catch (Exception ex)
            {
                // A popup that cannot be shown must not stop the refresh: the driver ends up
                // with correct data without an explanation, which beats stale data with one.
                Debug.WriteLine($"RouteSignalPopup: could not show. {ex.Message}");
            }

            await countdown;
        }

        private async Task RunCountdownAsync(int seconds)
        {
            try
            {
                for (var remaining = seconds; remaining > 0; remaining--)
                {
                    UpdateCountdown(remaining);

                    await Task.Delay(1000, _cancellation.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // The driver pressed the button. Nothing to do: the popup is already closing.
                return;
            }

            await CloseSafelyAsync();
        }

        private void UpdateCountdown(int remaining)
        {
            MainThread.BeginInvokeOnMainThread(() =>
                CountdownLabel.Text = remaining == 1
                    ? "Continuing in 1 second"
                    : $"Continuing in {remaining} seconds");
        }

        private async void OnActionClicked(object? sender, EventArgs e)
        {
            _cancellation.Cancel();

            await CloseSafelyAsync();
        }

        private async Task CloseSafelyAsync()
        {
            try
            {
                await CloseAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RouteSignalPopup: could not close. {ex.Message}");
            }
        }
    }
}
