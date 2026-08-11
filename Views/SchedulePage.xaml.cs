using Raphael.Driver.ViewModels;

namespace Raphael.Driver.Views;

public partial class SchedulePage : ContentPage
{
	public SchedulePage()
	{
		InitializeComponent();
        BindingContext = new ScheduleViewModel();
    }
}