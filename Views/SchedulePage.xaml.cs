using Mercurio.Driver.ViewModels;

namespace Mercurio.Driver.Views;

public partial class SchedulePage : ContentPage
{
	public SchedulePage()
	{
		InitializeComponent();
        BindingContext = new ScheduleViewModel();
    }
}