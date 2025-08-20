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
using Astari25.Services;
using Astari25.Models;
// need this for preferences
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using MauiLauncher = Microsoft.Maui.ApplicationModel.Launcher;

#if WINDOWS
using Microsoft.UI.Input;
using Windows.System;
using Windows.UI.Core;
#endif

namespace Astari25.Views;

public partial class GamePage : ContentPage
{

    // the view and Game state
    private readonly GamePageViewModel _viewModel;
    private IDispatcherTimer? _uiTimer;
    private bool _popupShown = false;

    public GamePage()
    {
        InitializeComponent();

        // build view model and bind the page
        _viewModel = new GamePageViewModel();
        BindingContext = _viewModel;

        // Wait until all the visual stuff is ready before wiring the UI
        // Trying to avoid 0 width / 0 height / player clamped to wrong place
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

            // 60 fps UI loop on the dispatcher thread
            _uiTimer = Dispatcher.CreateTimer();
            _uiTimer.Interval = TimeSpan.FromMilliseconds(16);
            _uiTimer.Tick += (_, __) => Tick();
            _uiTimer.Start();
        });



    // Popup so that Player can see what binds do what at the start
    // Only shows when running on Windows machine
    #if WINDOWS
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
                ShowWindowControlsPopup();
    #endif
        }

    // Small method that shows the Controls popup once on windows
    #if WINDOWS
        private async void ShowWindowControlsPopup()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await DisplayAlert("Controls", "Use Left/Right arrows to Move, Up arrow to Shoot", "OK"));
        }
    #endif


    // Every frame. Ui gets updated
    // Reads input, updates draw, redraws
    // Handles the Game over aswell
    private async void Tick()
    {
        // Input. Windows is Keyboard only
        // TOuch controls are there for Android
        #if WINDOWS
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    // Listen for physical keys at the same time. no need for eventlisteners
                    static bool IsDown(VirtualKey k) =>
                        (InputKeyboardSource.GetKeyStateForCurrentThread(k) &
                         CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

                    bool left  = IsDown(VirtualKey.Left)  || IsDown(VirtualKey.A);
                    bool right = IsDown(VirtualKey.Right) || IsDown(VirtualKey.D);
                    bool shoot = IsDown(VirtualKey.Up)    || IsDown(VirtualKey.S);

                    // Horizontal left or right ( -1  0  +1 )
                    _viewModel.Player.InputX = left ? -1f : right ? 1f : 0f;


                    // Fire with cooldown / limits in the View Model
                    if (shoot)
                        _viewModel.TryShoot(DateTime.UtcNow);

                    // This makes the player move immediately. make it feel less slugish
                    if (_viewModel.Player.InputX != 0f)
                    {
                        _viewModel.Player.X += _viewModel.Player.MaxSpeed * _viewModel.Player.InputX;
                        _viewModel.ClampPlayerToCanvas();
                    }
                }
        #endif

        // Simulation. makes changes. Ticks the game forward and advances the state of everything
        _viewModel.Update();

        // Render everything from the update
        GameCanvas?.Invalidate();


        // Game ending logic
        if (_viewModel.IsGameOver && !_popupShown)
        {
            _popupShown = true;
            _uiTimer?.Stop();

            bool restart = await DisplayAlert("Game Over", $"Final Score: {_viewModel.Score}", "Restart", "Main Menu");

            // Save a record of this run before reset or navigation
            await TextResultService.AppendAsync(
                DateTime.UtcNow,
                _viewModel.Score,
                Preferences.Get(nameof(AppSettings.Difficulty), "Normal")
            );

            var filePath = Path.Combine(FileSystem.AppDataDirectory, "results.txt");

            // Let the user copy or open the file
            var choice = await DisplayActionSheet(
                "Results saved",
                "Close",
                null,
                "Copy path",
                "Open file"
            );

            if (choice == "Copy path")
            {
                await Clipboard.Default.SetTextAsync(filePath);
                await DisplayAlert("Copied", "Path copied to clipboard.", "OK");
            }
            else if (choice == "Open file")
            {
                try
                {
                    await MauiLauncher.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(filePath)
                    });
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Open failed", $"Couldn't open file.\n{ex.Message}\n\nPath:\n{filePath}", "OK");
                }
            }

            // copy this folder path and then you can use Win+R and open the results.txt file
            await DisplayAlert("Results file folder", FileSystem.AppDataDirectory, "OK");

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

    // Whenver the canvas is measured . resize  changes play area
    private void OnCanvasSizeChanged(object? sender, EventArgs e)
    {
        if (GameCanvas == null) return;

        float width = (float)GameCanvas.Width;
#if WINDOWS
// Cap Playfield width on windows. It is then centered in xaml

        if (DeviceInfo.Platform == DevicePlatform.WinUI && width > 800f)
            width = 800f;
#endif
        _viewModel.CanvasWidth = width;
        _viewModel.CanvasHeight = (float)GameCanvas.Height;

        _viewModel.SetPlayerAtBottom();
        _viewModel.ClampPlayerToCanvas();
    }

    // Page is leaving, stop events and loop
    // Need to do this or the tick() can continue. timer won't reset
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (GameCanvas != null)
            GameCanvas.SizeChanged -= OnCanvasSizeChanged;
        _uiTimer?.Stop();
        _uiTimer = null;
    }

    // Slider input path if you bring  it back
    // Have a deadzone due to previous errors where therre was something like stick drift.
    // X values changing due to logic
    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        var v = (float)e.NewValue;
        if (Math.Abs(v) < _viewModel.Player.DeadZone) v = 0f;
        _viewModel.Player.InputX = v;
    }


    // Touch move pad
    // Has _panMax and _panMin which is -1 and +1 for inpputX
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

    // Move left 10, then call the clamp to make sure within game boundaries
    private void OnLeftClicked(object sender, EventArgs e)
    {
        _viewModel.Player.X -= 10;
        _viewModel.ClampPlayerToCanvas();
    }

    // Same as Left. Move Right and then Call clamp
    private void OnRightClicked(object sender, EventArgs e)
    {
        _viewModel.Player.X += 10;
        _viewModel.ClampPlayerToCanvas();
    }

    // This routes through the View Model. Has a cooldown and a limit per screen
    // That is all limited in the view model
    private void OnShootClicked(object sender, EventArgs e)
    {
        _viewModel.TryShoot(DateTime.UtcNow);
    }
}
