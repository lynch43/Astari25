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