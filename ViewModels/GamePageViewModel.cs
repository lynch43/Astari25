using Astari25.Views;
using Astari25.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using System.Linq;

namespace Astari25.ViewModels
{
    public class GamePageViewModel : INotifyPropertyChanged
    {
        public Player Player { get; } = new Player();
        public IDrawable GameDrawable { get; }

        // Spawning fields
        private int _framesInBetweensSpawns = 0;
        private int FramesPerSpawn => GetSpawnRateFromDifficulty();

        // Default Width for spawning probably not gonna work on windows. dont care for now
        public float CanvasWidth { get; set; } = 300f;
        public float CanvasHeight { get; set; } = 300f;

        public ObservableCollection<Bullet> Bullets { get; } = new ObservableCollection<Bullet>();

        public ObservableCollection<Enemy> Enemies { get; } = new ObservableCollection<Enemy>();

        public ObservableCollection<Explosion> Explosions { get; } = new();
        public ObservableCollection<KillConfirmed> KillPopups { get; } = new();

        public GamePageViewModel() {
            GameDrawable = new GameRenderer(Player, Bullets, Enemies, Explosions, KillPopups, PlayPad);
        }

        public float PlayPad { get; set; } = 23f;


        // Overrule Everything else and place player at bottom of Canvas
        public void SetPlayerAtBottom() {

            // padding
            Player.Y = CanvasHeight - PlayPad - Player.Radius;
        }




        public bool IsGameOver { get; set; } = false;

        // Need the event listener and method to handle it
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private int _score;
        private int _framesInBetweenSpawns;

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

            if (CanvasWidth < Player.Radius * 2) {
                // dont do anything until game loads correctly and the width is correct
                return;
            }

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

            // Link the slider movement from GPxaml
            //Player.X += Player.HorizontalSpeed;

            // Player.cs variables// Accelerate toward input
            Player.VelocityX += Player.InputX * Player.Acceleration;

            
            Player.VelocityX = Math.Clamp(Player.VelocityX, -Player.MaxSpeed, Player.MaxSpeed);
            
            
            if (Math.Abs(Player.InputX) < Player.DeadZone)
            {
                if (Player.VelocityX > 0) Player.VelocityX = Math.Max(0, Player.VelocityX - Player.Friction);
                else if (Player.VelocityX < 0) Player.VelocityX = Math.Min(0, Player.VelocityX + Player.Friction);
            }

            Player.X += Player.VelocityX;
            ClampPlayerToCanvas();


            foreach (var bullet in Bullets) {

                
                    foreach (var enemy in Enemies) {
                        if (IsColliding(bullet.X, bullet.Y, 5, enemy.X, enemy.Y, enemy.Radius)) {
                            bulletsToRemove.Add(bullet);
                            enemiesToRemove.Add(enemy);

                            Score += 10; // every hit amend the score on xaml
                            KillPopups.Add(new KillConfirmed(enemy.X, enemy.Y, 10, 24, 24));



                            Explosions.Add(new Explosion(enemy.X, enemy.Y));
                            //Console.WriteLine($"Got on. Score: {Score}");
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

            // need to get rid of the explosions
            foreach (var exp in Explosions.ToList()) {


                exp.Update();
                if (exp.IsDone)
                {
                    Explosions.Remove(exp);
                }
            }

            // update and then begin to tick
            foreach (var kc in KillPopups.ToList()) {
                kc.Update();
                if (kc.IsDone) {
                    KillPopups.Remove(kc);
                }
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

            if (_framesInBetweenSpawns >= FramesPerSpawn)
            {
                _framesInBetweenSpawns = 0;

                float r = 12f;
                float minX = PlayPad + r;
                float maxX = CanvasWidth - PlayPad - r;

                if (maxX > minX)
                {
                    float startX = Random.Shared.NextSingle() * (maxX - minX) + minX;
                    float startY = PlayPad + r;
                    //Enemies.Add(new Enemy(startX, startY));


                    var enemy = new Enemy(startX, startY);
                    enemy.ApplyDifficulty(Preferences.Get(nameof(AppSettings.Difficulty), "Normal"));
                    Enemies.Add(enemy);
                }
            }

            if (Player.Lives <= 0)
            {
                IsGameOver = true;
            }

            ClampPlayerToCanvas();


        }

        public void Reset() {

            Score = 0;
            Player.Lives = 3;


            Player.X = CanvasWidth / 2f;
            SetPlayerAtBottom();

            Bullets.Clear();
            Enemies.Clear();
            KillPopups.Clear();
            IsGameOver = false;
        }



        private int GetSpawnRateFromDifficulty() {

            var diff = Preferences.Get(nameof(AppSettings.Difficulty), "Normal");
            return diff switch {
                "Easy" => 150,
                "Normal" => 120,
                "Hard" => 80,
                _ => 120
            };
        }

        private float GetEnemySpeedMultiplier()
        {
            var diff = Preferences.Get(nameof(AppSettings.Difficulty), "Normal");
            return diff switch
            {
                "Easy" => 0.7f,
                "Normal" => 1f,
                "Hard" => 1.4f,
                _ => 1f
            };
        }




        // Stop the slider from sending Player off screen
        public void ClampPlayerToCanvas()
        {

            Player.X = Math.Clamp(Player.X, PlayPad + Player.Radius, CanvasWidth - PlayPad - Player.Radius);
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
