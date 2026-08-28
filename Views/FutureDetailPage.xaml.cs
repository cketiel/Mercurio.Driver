using Raphael.Driver.ViewModels;

namespace Raphael.Driver.Views;

public partial class FutureDetailPage : ContentPage
{
    /// <summary>
    /// ⚠️ The view model is injected and bound here. This page used to be built with a
    /// parameterless constructor and no binding context at all: Shell resolved it, the
    /// navigation parameter had nowhere to land, and every field on it came up empty.
    /// </summary>
    public FutureDetailPage(FutureDetailViewModel viewModel)
	{
		InitializeComponent();

        BindingContext = viewModel;
	}
}