using Raphael.Driver.ViewModels;

namespace Raphael.Driver.Views
{
    public partial class NotificationsPage : ContentPage
    {
        private readonly NotificationsViewModel _viewModel;

        public NotificationsPage(NotificationsViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // The live channel may have been down while the page was closed, and a push that
            // arrived with the app in the background never touched the list.
            await _viewModel.OnAppearingAsync();
        }
    }
}
