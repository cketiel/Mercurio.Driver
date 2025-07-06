using Mercurio.Driver.ViewModels;

namespace Mercurio.Driver.Views;

public partial class TodaySchedulePage : ContentPage
{
    private readonly TodayScheduleViewModel _viewModel;

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
    }
}