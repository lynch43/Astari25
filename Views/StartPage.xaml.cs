using System.Runtime.CompilerServices;

namespace Astari25.Views;

public partial class StartPage : ContentPage
{
	public StartPage()
	{
        InitializeComponent();


			
	}

    private async void OnStartGameClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GamePage());

    }

}