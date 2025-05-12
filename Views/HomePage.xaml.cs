namespace Mercurio.Driver.Views;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
	}

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        Preferences.Remove("AuthToken");
        await Shell.Current.GoToAsync("//LoginPage");
    }
}