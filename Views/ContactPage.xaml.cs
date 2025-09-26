namespace Mercurio.Driver.Views
{
    public partial class ContactPage : ContentPage
    {
        public ContactPage(ViewModels.ContactViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is ViewModels.ContactViewModel vm)
            {

                vm.InitializeCommand.Execute(null);
            }
        }
    }
}