using Mercurio.Driver.ViewModels;

namespace Mercurio.Driver.Views;

public partial class TodaySchedulePage : ContentPage
{
    public TodaySchedulePage(TodayScheduleViewModel viewModel)
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

    /*private readonly TodayScheduleViewModel _viewModel;

    public TodaySchedulePage(TodayScheduleViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Load events when page appears
        await _viewModel.LoadEventsCommand.ExecuteAsync(null);
    }*/
}