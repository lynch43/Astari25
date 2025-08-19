// File used for:
// State of Player at the time
// Movement settings
// Acceleration, Max Speed, Dead Zone, Friction
// Lives and Score will be rendered here from the View Model data

// Hold all player data used by Game Loop and renderer
// Shows Score and lives and allows UI to be updated
// Reset() gives us a clean start where values are set to defaults
// Lives and Score call OnProperty|Changed so labels bound in XAML update
// X/Y/Radius dont make any changes using events

// InputX is from Movepad or touch controls and can go from -1 to +1
// Clamp happens on ViewModel based on size of Canvas

using System.ComponentModel;

namespace Astari25.Models
{
    public class Player : INotifyPropertyChanged
    {
        // Lives remaining. This is showing in XAML
        private int _lives = 3;
        public int Lives
        {
            get => _lives;
            set
            {
                if (_lives != value)
                {
                    _lives = value;
                    OnPropertyChanged(nameof(Lives));
                }
            }
        }

        // - Current Score This is showing in XAML
        private int _score = 0;
        public int Score
        {
            get => _score;
            set
            {
                if (_score != value)
                {
                    _score = value;
                    OnPropertyChanged(nameof(Score));
                }
            }
        }

        // - HorizontalSpeed: optional direct speed. Messed around with this. it is staying at 0
        // - InputX: player input from controls -1 left +1 right
        // - VelocityX: Current  horizontal velocity used by the View Models Update{}
        public float HorizontalSpeed { get; set; } = 0;
        public float InputX { get; set; } = 0f;
        public float VelocityX { get; set; } = 0.3f;

        // Movement settings. Can mess with these to change it
        public float Acceleration { get; set; } = 0.3f;
        public float MaxSpeed { get; set; } = 6f;

        // Friction applied when InputX is within DeadZone
        // This was added because there was more than 0 values altering movement early during making project
        public float Friction { get; set; } = 1.0f;
        public float DeadZone { get; set; } = 0.05f;

        // Reset player to the baseline values for a new game
        public void Reset() {

            X = 300;
            Y = 550;
            Lives = 3;
            Score = 0;
            // This keeps the bindings in sync. reflecting on the XAML on new game
            OnPropertyChanged(nameof(Lives));
            OnPropertyChanged(nameof(Score));
        }

        // General poisition that is used in GameRenderer.cs
        public float X { get; set; } = 300;
        public float Y { get; set; } = 550;

        // Visual size for drawing and collision. Enemy and player
        public float Radius { get; set; } = 20;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}