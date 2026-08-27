using Raphael.Driver.DTOs;
using Raphael.Driver.Services;
using Raphael.Driver.ViewModels;

namespace Raphael.Driver.Views;

public partial class SignaturePage : ContentPage, IRouteSignalHandler
{
    private readonly RouteSignalCoordinator _signals;

    public SignaturePage(SignatureViewModel viewModel, RouteSignalCoordinator signals)
    {
        InitializeComponent();

        _signals = signals;

        BindingContext = viewModel;
    }

    // We force the horizontal orientation when the page appears
    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as SignatureViewModel)?.ForceLandscape();

        _signals.Register(this);
    }

    // We reverse the orientation when leaving the page
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        (BindingContext as SignatureViewModel)?.RestoreOrientation();

        _signals.Unregister(this);
    }

    /// <summary>
    /// Never here. The signal waits until the driver leaves this screen.
    /// </summary>
    /// <remarks>
    /// ⚠️ This page registers only in order to refuse. A patient has their finger on the
    /// screen: covering it with a popup loses the signature, and the signature is the proof
    /// the trip happened. Whatever changed on the route will still be true in a minute.
    /// </remarks>
    public Task<RouteSignalOutcome> HandleRouteSignalAsync(NotificationDto signal)
        => Task.FromResult(RouteSignalOutcome.Deferred);
}
