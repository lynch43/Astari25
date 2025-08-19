using Astari25.Models;
using Astari25.ViewModels;
using System.Timers;

#if WINDOWS
using Microsoft.UI.Input;
using Windows.System;
using Windows.UI.Core;
#endif

namespace Astari25.Views;

public partial class GamePage : ContentPage
{
    private readonly GamePageViewModel _viewModel;
    private readonly System.Timers.Timer _gameTimer;
    private bool _popupShown = false;

    public GamePage()
    {
        InitializeComponent();

        if (GameCanvas != null) {
            GameCanvas.SizeChanged += OnCanvasSizeChanged;
            if (_viewModel.GameDrawable != null)
                GameCanvas.Drawable = _viewModel.GameDrawable;

            _viewModel.CanvasWidth = (float)GameCanvas.Width;
        }
        

        _viewModel = new GamePageViewModel();
        BindingContext = _viewModel;
        GameCanvas.Drawable = _viewModel.GameDrawable;

        _viewModel.CanvasWidth = (float)GameCanvas.Width;

        _gameTimer = new System.Timers.Timer(16);
        _gameTimer.Elapsed += OnGameLoop;
        _gameTimer.Start();

        if (DeviceInfo.Platform == DevicePlatform.WinUI)
            ShowWindowControlsPopup();
    }

    private async void ShowWindowControlsPopup()
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await DisplayAlert("Controls", "Use Left/Right arrows to Move, Up arrow to Shoot", "OK");
        });
    }

    private async void OnGameLoop(object sender, ElapsedEventArgs e)
    {
        _viewModel.Update();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            // bail out if this is null and try again
            if (_viewModel.Player == null || _viewModel.Bullets == null) {
                return;
            }
#if WINDOWS
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                static bool IsDown(VirtualKey k) =>
                    (InputKeyboardSource.GetKeyStateForCurrentThread(k) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

                bool left  = IsDown(VirtualKey.Left)  || IsDown(VirtualKey.A);
                bool right = IsDown(VirtualKey.Right) || IsDown(VirtualKey.D);
                bool shoot = IsDown(VirtualKey.Up)    || IsDown(VirtualKey.S);

                _viewModel.Player.InputX = left ? -1f : right ? 1f : 0f;

                if (shoot)
                {
                    //_viewModel.Bullets.Add(new Bullet(_viewModel.Player.X, _viewModel.Player.Y));
                    _viewModel.TryShoot(DateTime.UtcNow);
                }

                if (_viewModel.Player.InputX != 0f)
                {
                    _viewModel.Player.X += _viewModel.Player.MaxSpeed * _viewModel.Player.InputX;
                    _viewModel.ClampPlayerToCanvas();
                }
            }
#endif
            if (GameCanvas == null) return;
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
                else
                {
                    _gameTimer.Stop();
                    await Shell.Current.GoToAsync("StartPage");
                }
            }
        });
    }

    private void OnCanvasSizeChanged(object sender, EventArgs e)
    {
        float width = (float)GameCanvas.Width;

        if (DeviceInfo.Platform == DevicePlatform.WinUI && width > 800f)
            width = 800f;

        _viewModel.CanvasWidth = width;
        _viewModel.CanvasHeight = (float)GameCanvas.Height;

        _viewModel.SetPlayerAtBottom();
        _viewModel.ClampPlayerToCanvas();

        Console.WriteLine($"Canvas Width update: {_viewModel.CanvasWidth}");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _gameTimer?.Stop();
        _gameTimer?.Dispose();
    }

    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        var v = (float)e.NewValue;
        if (Math.Abs(v) < _viewModel.Player.DeadZone)
            v = 0f;

        _viewModel.Player.InputX = v;
    }

    double _panMax = 120;
    private void OnMovePadPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _viewModel.Player.InputX = 0f;
                break;
            case GestureStatus.Running:
                var dx = Math.Clamp(e.TotalX, -_panMax, _panMax);
                _viewModel.Player.InputX = (float)(dx / _panMax);
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _viewModel.Player.InputX = 0f;
                break;
        }
    }

    private void OnLeftClicked(object sender, EventArgs e)
    {
        _viewModel.Player.X -= 10;
        _viewModel.ClampPlayerToCanvas();
    }

    private void OnRightClicked(object sender, EventArgs e)
    {
        _viewModel.Player.X += 10;
        _viewModel.ClampPlayerToCanvas();
    }

    private void OnShootClicked(object sender, EventArgs e)
    {
        //var bullet = new Bullet(_viewModel.Player.X, _viewModel.Player.Y);
        _viewModel.TryShoot(DateTime.UtcNow);
    }
}
