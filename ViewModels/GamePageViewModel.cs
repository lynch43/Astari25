using Astari25.Views;
using Astari25.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Astari25.ViewModels
{
    public class GamePageViewModel : INotifyPropertyChanged
    {
        public Player Player { get; } = new Player();
        public IDrawable GameDrawable { get; }

        // Spawning fields
        private int _framesInBetweenSpawns = 0;
        private const int FramesPerSpawn = 120;

        public ObservableCollection<Bullet> Bullets { get; } = new ObservableCollection<Bullet>();

        public ObservableCollection<Enemy> Enemies { get; } = new ObservableCollection<Enemy>();

        public GamePageViewModel() {
            GameDrawable = new GameRenderer(Player, Bullets, Enemies);
        }

        public bool IsGameOver { get; set; } = false;

        // Need the event listener and method to handle it
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private int _score;
        public int Score {
            get => _score;
            set {
                if (_score != value) {

                    _score = value;
                    OnPropertyChanged();
                }
            }
        }

        public void Update() {

            if ( Player.Lives <= 0)
            {
                IsGameOver = true;
                //Console.WriteLine("You ran out of lives");
            }
            // go right slow
            //Player.X+=2;

            // update the collection
            foreach (var bullet in Bullets.ToList()) {
                bullet.Update();
            }

            // store bullets and enemies that need to be removed
            var bulletsToRemove = new List<Bullet>();
            var enemiesToRemove = new List<Enemy>();

            foreach (var bullet in Bullets) {

                
                    foreach (var enemy in Enemies) {
                        if (IsColliding(bullet.X, bullet.Y, 5, enemy.X, enemy.Y, enemy.Radius)) {
                            bulletsToRemove.Add(bullet);
                            enemiesToRemove.Add(enemy);

                            Score += 10; // every hit amend the score on xaml
                            Console.WriteLine($"Got on. Score: {Score}");
                            break;
                        }
                    }

            }

            foreach (var bullet in bulletsToRemove) {
                Bullets.Remove(bullet);
            }

            foreach (var enemy in enemiesToRemove) {
                Enemies.Remove(enemy);
            }


            foreach (var enemy in Enemies.ToList()) {
                enemy.Update();
            }

            // lose a life if things get to the astari base
            var escapedEnemies = new List<Enemy>();
            
            foreach (var enemy in Enemies) {
                if (enemy.Y > Player.Y + Player.Radius + 10)
                {
                    //Console.WriteLine("Enemy escaped! Lose a life.");
                    Player.Lives--;
                    escapedEnemies.Add(enemy);
                }
            }
            // remove the enemy outside of list so i ament looping and removing at the same time
            foreach (var enemy in escapedEnemies) {
                Enemies.Remove(enemy);
            }

            foreach (var enemy in Enemies.ToList())
            {
                float dx = Player.X - enemy.X;
                float dy = Player.Y - enemy.Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                if (distance < enemy.Radius + Player.Radius)
                {
                    //Console.WriteLine("Player hit, lose a life");
                    Player.Lives--;

                    Enemies.Remove(enemy);

                }
            }


            // TODO. Enemies spawning at random. game is getting overwhelmed and stutterring
            // SPAWN LOGIC
            _framesInBetweenSpawns++;

            if (_framesInBetweenSpawns >= FramesPerSpawn) {

                _framesInBetweenSpawns = 0;

                float padding = 20f;
                float canvasWidth = 500f;
                float startX = Random.Shared.NextSingle() * (canvasWidth - 2 * padding) + padding;

                Enemies.Add(new Enemy(startX, 0));
            }

            if (Player.Lives <= 0)
            {
                //Console.WriteLine("Out of lives");
                IsGameOver = true;
            }


        }

        public void Reset() {

            Score = 0;
            Player.Lives = 3;
            Player.X = 300;
            Player.Y = 500;

            Bullets.Clear();
            Enemies.Clear();

            IsGameOver = false;
        }

        private bool IsColliding(float x1, float y1, float r1, float x2, float y2, float r2)
        {
            float dx = x1 - x2;
            float dy = y1 - y2;
            float distanceSquared = dx * dx + dy * dy;
            float radiusSum = r1 + r2;

            return distanceSquared <= radiusSum * radiusSum;

        }


    }


}
