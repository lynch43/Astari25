using Astari25.ViewModels;
using System.Timers;

namespace Astari25.Views;

public partial class GamePage : ContentPage
{
    private readonly GamePageViewModel _viewModel;
    private readonly System.Timers.Timer _gameTimer;


    public GamePage()
    {
        InitializeComponent();

        _viewModel = new GamePageViewModel();
        BindingContext = _viewModel;
        GameCanvas.Drawable = _viewModel.GameDrawable;

        // Setup game loop with System.Timers.Timer 60 frames
        _gameTimer = new System.Timers.Timer(16);
        _gameTimer.Elapsed += OnGameLoop;
        _gameTimer.Start();
    }

    private void OnGameLoop(object sender, ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            GameCanvas.Invalidate();
        });
    }
}
