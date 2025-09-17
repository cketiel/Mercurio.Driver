using Mercurio.Driver.ViewModels;

namespace Mercurio.Driver.Views;

public partial class PullOutDetailPage : ContentPage
{
    public PullOutDetailPage(PullOutDetailPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel; 
    }
    protected override bool OnBackButtonPressed()
    {       
        if (BindingContext is PullOutDetailPageViewModel vm)
        {
            // If the GoBack command can be executed...
            if (vm.GoBackCommand.CanExecute(null))
            {               
                vm.GoBackCommand.Execute(null);
            }
        }

        // We return 'true' to indicate that we have handled the event
        // and prevent the system from trying to close the app.
        return true;
    }
}