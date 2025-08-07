using Astari25.Models;
using Astari25.ViewModels;
using System.Timers;

namespace Astari25.Views;

// Main game screen
public partial class GamePage : ContentPage
{
    private readonly GamePageViewModel _viewModel;

    // Game Loop that runs 60 times a second
    private readonly System.Timers.Timer _gameTimer;

    public GamePage()
    {
        InitializeComponent();

        // Setup the ViewModel and binding for graphics
        _viewModel = new GamePageViewModel();
        BindingContext = _viewModel;
        GameCanvas.Drawable = _viewModel.GameDrawable;

        // Setup game loop with System.Timers.Timer (60 FPS)
        _gameTimer = new System.Timers.Timer(16);
        _gameTimer.Elapsed += OnGameLoop;
        _gameTimer.Start();
    }

    // This runs every frame (60 per second)
    // It updates the game logic and redraws the screen
    private void OnGameLoop(object sender, ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _viewModel.Update();
            GameCanvas.Invalidate();

            if (_viewModel.IsGameOver)
            {
                _gameTimer.Stop();

                bool result = await DisplayAlert("Game Over", $"Final Score: {_viewModel.Score}", "Restart","Cancel");
                if (result)
                {
                    // restart the game on same screen
                    _viewModel.Reset(); 
                    _gameTimer.Start();
                }
            }
        });
    }

    private void OnUpClicked(object sender, EventArgs e)
    {
        _viewModel.Player.Y -= 10;
    }

    private void OnDownClicked(object sender, EventArgs e)
    {
        _viewModel.Player.Y += 10;
    }

    private void OnLeftClicked(object sender, EventArgs e)
    {
        _viewModel.Player.X -= 10;
    }

    private void OnRightClicked(object sender, EventArgs e)
    {
        _viewModel.Player.X += 10;
    }

    // Pew (fire a single bullet)
    private void OnShootClicked(object sender, EventArgs e)
    {
        var bullet = new Bullet(_viewModel.Player.X, _viewModel.Player.Y);
        _viewModel.Bullets.Add(bullet);
    }
}
