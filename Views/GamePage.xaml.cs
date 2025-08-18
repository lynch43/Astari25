using Astari25.Models;
using Astari25.ViewModels;
using System.Timers;

#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
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

        // Track canvas size changes 
        GameCanvas.SizeChanged += OnCanvasSizeChanged;

        _viewModel = new GamePageViewModel();
        BindingContext = _viewModel;
        GameCanvas.Drawable = _viewModel.GameDrawable;

        _viewModel.CanvasWidth = (float)GameCanvas.Width;

        // Start main game loop (~60 FPS)
        _gameTimer = new System.Timers.Timer(16);
        _gameTimer.Elapsed += OnGameLoop;
        _gameTimer.Start();

        // Windows Only: popup
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

    #if WINDOWS
    if (DeviceInfo.Platform == DevicePlatform.WinUI)
    {
        var nativeWindow = this.Window?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
        if (nativeWindow?.Content is UIElement root)
        {
            // Hook up to windows root element
            root.KeyDown += OnKeyDown;
            root.KeyUp += OnKeyUp;
        }
    }
    #endif
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

    #if WINDOWS
    if (DeviceInfo.Platform == DevicePlatform.WinUI)
    {
        var nativeWindow = this.Window?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
        if (nativeWindow?.Content is UIElement root)
        {
            // unhook from button press
            root.KeyDown -= OnKeyDown;
            root.KeyUp   -= OnKeyUp;
        }
    }
    #endif

        _gameTimer?.Stop();
        _gameTimer?.Dispose();
    }
    #if WINDOWS
    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        
        switch (e.Key)
        {
            case Windows.System.VirtualKey.Left:
            case Windows.System.VirtualKey.A:
                _viewModel.Player.InputX = -1f; 
                break;

            case Windows.System.VirtualKey.Right:
            case Windows.System.VirtualKey.D:
                _viewModel.Player.InputX = 1f;  break;

            case Windows.System.VirtualKey.Up:
            case Windows.System.VirtualKey.S:
                var bullet = new Bullet(_viewModel.Player.X, _viewModel.Player.Y);
                _viewModel.Bullets.Add(bullet);
                break;
        }
        Console.WriteLine($"Key down: {e.Key}");
    }

    private void OnKeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key is Windows.System.VirtualKey.Left or Windows.System.VirtualKey.Right
                     or Windows.System.VirtualKey.A    or Windows.System.VirtualKey.D)
        {
            _viewModel.Player.InputX = 0f;
        }
        Console.WriteLine($"Key up: {e.Key}");
    }
    #endif


    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        var v = (float)e.NewValue;
        if (Math.Abs(v) < _viewModel.Player.DeadZone)
            v = 0f;

        _viewModel.Player.InputX = v;
    }

    double _panMax = 120; // mapping drag distance to full speed
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
        var bullet = new Bullet(_viewModel.Player.X, _viewModel.Player.Y);
        _viewModel.Bullets.Add(bullet);
    }
}
