// File used for:
// - Wire the view model, set Drawable so we can print to screen, tracks the size of canvas
// - Runs the UI Game Loop
// - Handles the Windows Keyboard touches and the Touch screen controls for Android
// Issues that should be made aware:
// - Itialize after layout: UI first then logic
// - Checking For Null has been an issue. And padding sizes indexing issues
// - Invalidate() goes after Update()


using Astari25.Models;
using Astari25.ViewModels;

#if WINDOWS
using Microsoft.UI.Input;
using Windows.System;
using Windows.UI.Core;
#endif

namespace Astari25.Views;

public partial class GamePage : ContentPage
{
    private readonly GamePageViewModel _viewModel;
    private IDispatcherTimer? _uiTimer;
    private bool _popupShown = false;

    public GamePage()
    {
        InitializeComponent();

        _viewModel = new GamePageViewModel();
        BindingContext = _viewModel;

        Dispatcher.Dispatch(() =>
        {
            if (GameCanvas != null)
            {
                GameCanvas.SizeChanged += OnCanvasSizeChanged;
                GameCanvas.Drawable = _viewModel.GameDrawable;

                _viewModel.CanvasWidth = (float)GameCanvas.Width;
                _viewModel.CanvasHeight = (float)GameCanvas.Height;
                _viewModel.SetPlayerAtBottom();
                _viewModel.ClampPlayerToCanvas();
            }

            _uiTimer = Dispatcher.CreateTimer();
            _uiTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 fps
            _uiTimer.Tick += (_, __) => Tick();
            _uiTimer.Start();
        });

#if WINDOWS
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
            ShowWindowControlsPopup();
#endif
    }

#if WINDOWS
    private async void ShowWindowControlsPopup()
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
            await DisplayAlert("Controls", "Use Left/Right arrows to Move, Up arrow to Shoot", "OK"));
    }
#endif

    private async void Tick()
    {
        // Everything on UI thread
#if WINDOWS
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            static bool IsDown(VirtualKey k) =>
                (InputKeyboardSource.GetKeyStateForCurrentThread(k) &
                 CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

            bool left  = IsDown(VirtualKey.Left)  || IsDown(VirtualKey.A);
            bool right = IsDown(VirtualKey.Right) || IsDown(VirtualKey.D);
            bool shoot = IsDown(VirtualKey.Up)    || IsDown(VirtualKey.S);

            _viewModel.Player.InputX = left ? -1f : right ? 1f : 0f;

            if (shoot)
                _viewModel.TryShoot(DateTime.UtcNow);

            if (_viewModel.Player.InputX != 0f)
            {
                _viewModel.Player.X += _viewModel.Player.MaxSpeed * _viewModel.Player.InputX;
                _viewModel.ClampPlayerToCanvas();
            }
        }
#endif

        _viewModel.Update();

        GameCanvas?.Invalidate();

        if (_viewModel.IsGameOver && !_popupShown)
        {
            _popupShown = true;
            _uiTimer?.Stop();

            bool restart = await DisplayAlert("Game Over", $"Final Score: {_viewModel.Score}", "Restart", "Main Menu");
            if (restart)
            {
                _viewModel.Reset();
                _popupShown = false;
                _uiTimer?.Start();
            }
            else
            {
                _uiTimer?.Stop();
                await Shell.Current.GoToAsync("StartPage");
            }
        }
    }

    private void OnCanvasSizeChanged(object? sender, EventArgs e)
    {
        if (GameCanvas == null) return;

        float width = (float)GameCanvas.Width;
#if WINDOWS
        if (DeviceInfo.Platform == DevicePlatform.WinUI && width > 800f)
            width = 800f;
#endif
        _viewModel.CanvasWidth = width;
        _viewModel.CanvasHeight = (float)GameCanvas.Height;

        _viewModel.SetPlayerAtBottom();
        _viewModel.ClampPlayerToCanvas();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (GameCanvas != null)
            GameCanvas.SizeChanged -= OnCanvasSizeChanged;
        _uiTimer?.Stop();
        _uiTimer = null;
    }

    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        var v = (float)e.NewValue;
        if (Math.Abs(v) < _viewModel.Player.DeadZone) v = 0f;
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
            default:
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
        _viewModel.TryShoot(DateTime.UtcNow);
    }
}
