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

        GameCanvas.SizeChanged += OnCanvasSizeChanged;

        _viewModel = new GamePageViewModel();
        BindingContext = _viewModel;
        GameCanvas.Drawable = _viewModel.GameDrawable;

        
        _viewModel.CanvasWidth = (float)GameCanvas.Width;


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

                bool result = await DisplayAlert("Game Over", $"Final Score: {_viewModel.Score}", "Restart", "Main Menu");
                if (result)
                {
                    _viewModel.Reset();
                    _popupShown = false;
                    _gameTimer.Start();
                }
                else {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        _gameTimer.Stop();
                        

                    });

                    await Shell.Current.GoToAsync("StartPage");
                }
            }
        });
    }

    // Change canvas size
    
    // size to the phone' width
    private void OnCanvasSizeChanged(object sender, EventArgs e) {

        _viewModel.CanvasWidth = (float)GameCanvas.Width;
        _viewModel.CanvasHeight = (float)GameCanvas.Height;

        _viewModel.SetPlayerAtBottom();
        _viewModel.ClampPlayerToCanvas();

        // debug
        Console.WriteLine($"Canvas Width update: {_viewModel.CanvasWidth}");

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
        _viewModel.Player.InputX = (float)e.NewValue;
        // everything back to zero. stops player flying off screen on start
        var v = (float)e.NewValue;
        if (Math.Abs(v) < _viewModel.Player.DeadZone) {
            v = 0f;
        }
        _viewModel.Player.InputX = v;

    }

    //private void OnSliderReleased(object sender, EventArgs e) {

    //    if (MoveSlider.Value != 0) {
    //        MainThread.BeginInvokeOnMainThread(() => MoveSlider.Value = 0);
    //        Console.WriteLine($"ALERT LOOKING FOR STICK DRIFT -> InputX={_viewModel.Player.InputX}");
    //    }

    //    _viewModel.Player.InputX = 0f;
    //}

    // MOVEPAD
    double _panMax = 120; // how far dragging maps to full speed (tweak later)

    private void OnMovePadPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _viewModel.Player.InputX = 0f;
                break;

            case GestureStatus.Running:
                // Same as the slider left, right drag to [-1 : 1]
                var dx = Math.Clamp(e.TotalX, -_panMax, _panMax);
                float input = (float)(dx / _panMax);
                _viewModel.Player.InputX = input;
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _viewModel.Player.InputX = 0f;     // let go -> stop
                break;
        }
    }
    private void OnLeftClicked(object sender, EventArgs e)
    {
        _viewModel.Player.X -= 10;
        _viewModel.ClampPlayerToCanvas();


    }
    private void OnRightClicked(object sender, EventArgs e) {
        _viewModel.Player.X += 10;
        _viewModel.ClampPlayerToCanvas();
    } 

    private void OnShootClicked(object sender, EventArgs e)
    {
        var bullet = new Bullet(_viewModel.Player.X, _viewModel.Player.Y);
        _viewModel.Bullets.Add(bullet);
    }
}