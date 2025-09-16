using Mercurio.Driver.ViewModels;

namespace Mercurio.Driver.Views;

public partial class SignaturePage : ContentPage
{
    public SignaturePage(SignatureViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // We force the horizontal orientation when the page appears
    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as SignatureViewModel)?.ForceLandscape();
    }

    // We reverse the orientation when leaving the page
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        (BindingContext as SignatureViewModel)?.RestoreOrientation();
    }
}