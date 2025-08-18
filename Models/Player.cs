using System.ComponentModel;

namespace Astari25.Models
{
    public class Player : INotifyPropertyChanged
    {
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

        public float HorizontalSpeed { get; set; } = 0;
        public float InputX { get; set; } = 0f; // slider can 1 up or down 1
        public float VelocityX { get; set; } = 0.3f;
        public float Acceleration { get; set; } = 0.3f;
        public float MaxSpeed { get; set; } = 6f;

        // 
        public float Friction { get; set; } = 1.0f;
        public float DeadZone { get; set; } = 0.05f;

        public void Reset() {

            X = 300;
            Y = 550;
            Lives = 3;
            Score = 0;
            OnPropertyChanged(nameof(Lives)); // might remove
            OnPropertyChanged(nameof(Score)); // might remove dunno if this is needed
        }

        public float X { get; set; } = 300;
        public float Y { get; set; } = 550;
        public float Radius { get; set; } = 20;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}