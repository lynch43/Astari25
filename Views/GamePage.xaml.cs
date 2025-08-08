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

        GameCanvas.SizeChanged += OnCanvasSizeChanged;

        _gameTimer = new System.Timers.Timer(16); // back to 60fps
        _gameTimer.Elapsed += OnGameLoop;
        _gameTimer.Start();
    }

    private async void OnGameLoop(object sender, ElapsedEventArgs e)
    {
        _viewModel.Update();

        MainThread.BeginInvokeOnMainThread(async () =>
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

    // Change canvas size
    

    private void OnCanvasSizeChanged(object sender, EventArgs e) {
        _viewModel.CanvasWidth = (float)GameCanvas.Width;
    }


    // Stop any timers from ticking after game closes
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _gameTimer?.Stop();
        _gameTimer?.Dispose();
    }

    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        _viewModel.Player.HorizontalSpeed = (float)e.NewValue * 5f;
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