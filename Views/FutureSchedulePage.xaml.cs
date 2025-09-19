using Mercurio.Driver.ViewModels;

namespace Mercurio.Driver.Views;

public partial class FutureSchedulePage : ContentPage
{
	public FutureSchedulePage(FutureScheduleViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // We check if the BindingContext is of the correct type and call the command to reload.
        if (BindingContext is TodayScheduleViewModel vm && vm.LoadEventsCommand.CanExecute(null))
        {
            vm.LoadEventsCommand.Execute(null);
        }
    }
}