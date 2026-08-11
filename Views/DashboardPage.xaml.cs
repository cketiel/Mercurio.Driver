using Raphael.Driver.ViewModels;

namespace Raphael.Driver.Views;

public partial class DashboardPage : ContentPage
{
	public DashboardPage(DashboardViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
  
}