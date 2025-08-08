using Astari25.Views;
namespace Astari25;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("StartPage", typeof(Astari25.Views.StartPage));
		Routing.RegisterRoute("GamePage", typeof(Astari25.Views.GamePage));
		Routing.RegisterRoute("SettingsPage", typeof(Astari25.Views.SettingsPage));
	}
}
