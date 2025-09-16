
using Mercurio.Driver.ViewModels;

namespace Mercurio.Driver.Views;

public partial class EventDetailPage : ContentPage
{
    public EventDetailPage(EventDetailPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}