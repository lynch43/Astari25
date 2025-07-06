using Astari25.Views;

namespace Astari25;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		// NavigationPage is there for .PushAsync()
		MainPage = new NavigationPage(new StartPage());
	}

}