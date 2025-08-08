using Astari25.Views;

namespace Astari25;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		// Need to switch this now to use GoToAsync
		MainPage = new AppShell();
	}

}