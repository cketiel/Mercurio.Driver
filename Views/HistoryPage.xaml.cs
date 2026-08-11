using Raphael.Driver.ViewModels;

namespace Raphael.Driver.Views;

public partial class HistoryPage : ContentPage
{
	public HistoryPage(HistoryViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}