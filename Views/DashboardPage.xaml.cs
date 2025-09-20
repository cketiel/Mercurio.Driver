using Mercurio.Driver.ViewModels;

namespace Mercurio.Driver.Views;

public partial class DashboardPage : ContentPage
{
	public DashboardPage(DashboardViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
  
}