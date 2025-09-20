using Mercurio.Driver.ViewModels;

namespace Mercurio.Driver.Views;

public partial class HistoryPage : ContentPage
{
	public HistoryPage(HistoryViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}