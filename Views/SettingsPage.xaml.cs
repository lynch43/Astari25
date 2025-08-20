using Astari25.ViewModels;
namespace Astari25.Views;

// File used for:
// - code behind for the settings creen
// - Creates and attaches to the SettingsViewModel so the page can bind data
// Just initializes the View and wires up the BindingContext
public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();

		// Binds the page to its view model so zaml controls are able to read and write
		BindingContext = new SettingsViewModel();
	}
}