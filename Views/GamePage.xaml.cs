using Astari25.Models;
using Astari25.ViewModels;
using System.Timers;

namespace Astari25.Views;

public partial class GamePage : ContentPage
{
    private readonly GamePageViewModel _viewModel;
    private readonly System.Timers.Timer _gameTimer;
    private bool _popupShown = false;

    public GamePage()
    {
        InitializeComponent();

        _viewModel = new GamePageViewModel();
        BindingContext = _viewModel;
        GameCanvas.Drawable = _viewModel.GameDrawable;

        _gameTimer = new System.Timers.Timer(33); // ~30 FPS
        _gameTimer.Elapsed += OnGameLoop;
        _gameTimer.Start();
    }

    private async void OnGameLoop(object sender, ElapsedEventArgs e)
    {
        // Off the UI thread - do heavy lifting here
        _viewModel.Update();

        // Now go to UI thread only for drawing and alerts
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            GameCanvas.Invalidate();

            if (_viewModel.IsGameOver && !_popupShown)
            {
                _popupShown = true;
                _gameTimer.Stop();

                bool result = await DisplayAlert("Game Over", $"Final Score: {_viewModel.Score}", "Restart", "Cancel");
                if (result)
                {
                    _viewModel.Reset();
                    _popupShown = false;
                    _gameTimer.Start();
                }
            }
        });
    }

    private void OnUpClicked(object sender, EventArgs e) => _viewModel.Player.Y -= 10;
    private void OnDownClicked(object sender, EventArgs e) => _viewModel.Player.Y += 10;
    private void OnLeftClicked(object sender, EventArgs e) => _viewModel.Player.X -= 10;
    private void OnRightClicked(object sender, EventArgs e) => _viewModel.Player.X += 10;

    private void OnShootClicked(object sender, EventArgs e)
    {
        var bullet = new Bullet(_viewModel.Player.X, _viewModel.Player.Y);
        _viewModel.Bullets.Add(bullet);
    }
}