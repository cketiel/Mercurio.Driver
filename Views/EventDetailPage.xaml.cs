
using Raphael.Driver.ViewModels;

namespace Raphael.Driver.Views;

public partial class EventDetailPage : ContentPage
{
    public EventDetailPage(EventDetailPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}