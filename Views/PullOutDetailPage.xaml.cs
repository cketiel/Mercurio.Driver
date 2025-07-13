using Mercurio.Driver.ViewModels;

namespace Mercurio.Driver.Views;

public partial class PullOutDetailPage : ContentPage
{
    public PullOutDetailPage(PullOutDetailPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}