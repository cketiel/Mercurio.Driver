using Mercurio.Driver.ViewModels;

namespace Mercurio.Driver.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
        BindingContext = new LoginViewModel();
    }
    
}