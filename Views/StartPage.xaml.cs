using System;
namespace Astari25.Views;

// File used for
// - The start screen. landing page. 
// - Two button Start game and Settings
// - Navigation pushes the page onto the stack
public partial class StartPage : ContentPage
{
    public StartPage()
    {
        // start up the Xaml
        InitializeComponent();
    }

    // Start button, navigates to the game screen

    private async void OnStartGameClicked(object sender, EventArgs e)
    {
        // Everything has to be in a NavigationPage to use the PushToAsync
        await Navigation.PushAsync(new GamePage());
    }

    // Settings button, navigates to the settings screen
    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage());
    }
}