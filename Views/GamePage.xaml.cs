using Astari25.Models;
using Astari25.ViewModels;
using System.Timers;

namespace Astari25.Views;

// main game screen
public partial class GamePage : ContentPage
{
    private readonly GamePageViewModel _viewModel;
    // Game Loop that runs 60 times a second
    private readonly System.Timers.Timer _gameTimer;


    public GamePage()
    {
        InitializeComponent();

        // setup the View Model and binding for graphics
        _viewModel = new GamePageViewModel();
        BindingContext = _viewModel;
        GameCanvas.Drawable = _viewModel.GameDrawable;

        // Setup game loop with System.Timers.Timer 60 frames
        _gameTimer = new System.Timers.Timer(16);

        // timer gets connected to the game loop function
        _gameTimer.Elapsed += OnGameLoop;
        _gameTimer.Start(); // start ticking the timer
    }

    // This is running ever frame. 60 per sec
    // It update the game logic and redraws the screen
    private void OnGameLoop(object sender, ElapsedEventArgs e)
    {

        // all of the UI things that change need to run here. the main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _viewModel.Update(); // moves the player
            GameCanvas.Invalidate(); // makes the game reprint the Canvas
        });
    }

    private void OnUpClicked(object sender, EventArgs e) {

        _viewModel.Player.Y -= 10;

    }

    private void OnDownClicked(object sender, EventArgs e)
    {
        _viewModel.Player.Y += 10;
    }

    private void OnLeftClicked(object sender, EventArgs e) { 
        _viewModel.Player.X -= 10;
    }

    private void OnRightClicked(object sender, EventArgs e) { 
    
        _viewModel.Player.X += 10;
    }


    // pew singular
    private void OnShootClicked(object sender, EventArgs e) {
        //Console.WriteLine("Pew");
        // Creat a new bullet from player
        var bullet = new Bullet(_viewModel.Player.X, _viewModel.Player.Y);

        // Add the bullet to the ViewModel list
        _viewModel.Bullets.Add(bullet);
    }


}

