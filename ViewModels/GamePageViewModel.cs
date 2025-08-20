// File used for:
// - Game State and Rules of game
// - Owns Enemies , Player, Bullets / Enemies, Score, Spawn, Collisions
// - Update() runs on UI Thread set to a timer ( IDispatcherTimer ) 
// - ObservableCollections get changed safely throught the game loop
// - Input: Player input ( Player.InputX ) from page.
// - Shooting via TryShoot() (cooldown with limited bullets over time)

// - Single source of truth for the game. The View binds to this MVVM
// - Creates the IDrawable / GameRenderer but draw doesn't happen here.
//   It happens in the Renderer

// - GraphicsView reads from the colelctions when it draws
// Any list that should be changed is using .ToList() to avoid errors

using Astari25.Views;
using Astari25.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Storage;
//using MetalPerformanceShaders;

namespace Astari25.ViewModels
{
    public class GamePageViewModel : INotifyPropertyChanged
    {
        // Player state, position, velocity, lives, score parameters are all in Player
        public Player Player { get; } = new Player();

        // Renderer that the view plufs into, pure drawing no logic stuff
        public IDrawable GameDrawable { get; }

        // how quick everything spawns  NOT FINISHED
        private int _framesInBetweenSpawns;
        private int FramesPerSpawn => GetSpawnRateFromDifficulty();

        // constants
        private const int MinSpawnFrames = 24;
        private const int BaseEnemyCap = 10;
        private const int MaxEnemyCap = 30;


        // difficuoloty stae, changeable
        private DateTime _lastUpdateUtc = DateTime.MinValue;
        private double _elapsedSecondsTotal = 0;

        // Every frame
        private int _dynamicSpawnFrames;
        private float _speedMultiplier = 1f;
        private int _enemySoftCap;



        // Canvas Bounds, set by the page after layout
        // Used for clamping player in place and where enemies can spawn
        public float CanvasWidth { get; set; } = 300f;
        public float CanvasHeight { get; set; } = 300f;

        // Shooting Rules
        public int MaxBulletsOnScreen { get; set; } = 5; // capped to stop spamming
        TimeSpan _fireCooldown = TimeSpan.FromMilliseconds(180); // amount of time between shots
        DateTime _nextShotAtUtc = DateTime.MinValue; // when bullet is available to be shot


        // These are live collections
        // They are constantly being observed by the View
        public ObservableCollection<Bullet> Bullets { get; } = new ObservableCollection<Bullet>();
        public ObservableCollection<Enemy> Enemies { get; } = new ObservableCollection<Enemy>();
        public ObservableCollection<Explosion> Explosions { get; } = new();
        public ObservableCollection<KillConfirmed> KillPopups { get; } = new();


        // Construct and wire with live state references
        public GamePageViewModel()
        {
            GameDrawable = new GameRenderer(Player, Bullets, Enemies, Explosions, KillPopups, PlayPad);
        }

        // - Visual margin used for play area and Clamping
        public float PlayPad { get; set; } = 24f;
        
        // - Places player at bottom of canvas with a small padding
        public void SetPlayerAtBottom()
        {
            float bottomPadding = 35f;
            Player.Y = CanvasHeight - bottomPadding - Player.Radius;
        }


        // - Used to limit parameters of shooting
        // - Like a gate before a shot is made
        // - Enforces on screen buller cap
        // - Purnes off screen bullets first so the cap is actual bullets
        public bool TryShoot(DateTime nowUtc)
        {
            if (nowUtc < _nextShotAtUtc) return false;

            PruneBullets();
            if (Bullets.Count >= MaxBulletsOnScreen) return false;

            Bullets.Add(new Bullet(Player.X, Player.Y));
            _nextShotAtUtc = nowUtc + _fireCooldown;
            return true;
        }

        // Remove bullets that have gone above the screen to keep the list from getting too big
        private void PruneBullets()
        {
            for (int i = Bullets.Count - 1; i >= 0; i--)
            {
                if (Bullets[i].Y < -20)
                    Bullets.RemoveAt(i);
            }
        }

        // Base difficulty is starting point and take the values from there
        private void UpdateDifficulty() {
            // Get value from preferences for difficulty
            int baseSpawn = GetSpawnRateFromDifficulty();

            double t = _elapsedSecondsTotal;

            // time between spawns gets lower to a certain point
            int reduction = (int)Math.Min(baseSpawn - MinSpawnFrames, t * 1.25);
            _dynamicSpawnFrames = Math.Max(MinSpawnFrames, baseSpawn - reduction);

            // enemies go faster over time
            double speed =Math.Min(2.5, 0.9 * t / 45.0);
            _speedMultiplier = (float)speed;

            // enemy cap goes up as time goes on
            _enemySoftCap = (int)Math.Min(MaxEnemyCap, BaseEnemyCap + Math.Floor(t / 8.0));
        }

        // Game over flag. checked on the page to ensure the game should still be running
        public bool IsGameOver { get; set; } = false;

        // Use this event handler for bindable stuff like Score
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Score property . lets the ui know
        private int _score;
        public int Score
        {
            get => _score;
            set
            {
                if (_score != value)
                {
                    _score = value;
                    OnPropertyChanged();
                }
            }
        }

        // One frame of game logic and this is called
        // Order: 
        // 1) Exit on repeat until canvas has an actual width. stops the clamp from being thrown off
        // 2) Check lives => Game over
        // 3) Bullets can be iterated
        // 4) Move player from the input taking into acocunt the , acceleration, valocity, clamp
        // 5) Bullet to enemy collision , then score -> explosions -> kill popups, removal
        // 6) Enemies can move, collisions, score pop uup
        // 7) Escape and player collisions, lose a live + remove.
        // 8) Spawn new enemies by difficulty
        // 9) check game over. clamp again to avoid player movement
        public void Update()
        {
            var now = DateTime.UtcNow;
            if (_lastUpdateUtc == DateTime.MinValue) _lastUpdateUtc = now;
            double dt = (now - _lastUpdateUtc).TotalSeconds;
            _lastUpdateUtc = now;
            _elapsedSecondsTotal += dt;

            UpdateDifficulty();


            if (CanvasWidth < Player.Radius * 2) return;

            if (Player.Lives <= 0)
            {
                IsGameOver = true;
            }

            foreach (var bullet in Bullets.ToList())
            {
                bullet.Update();
            }

            var bulletsToRemove = new List<Bullet>();
            var enemiesToRemove = new List<Enemy>();


            // Movement : input => input => acceleration => velocity => if input is near zero then friction
            Player.VelocityX += Player.InputX * Player.Acceleration;
            Player.VelocityX = Math.Clamp(Player.VelocityX, -Player.MaxSpeed, Player.MaxSpeed);

            if (Math.Abs(Player.InputX) < Player.DeadZone)
            {
                if (Player.VelocityX > 0) Player.VelocityX = Math.Max(0, Player.VelocityX - Player.Friction);
                else if (Player.VelocityX < 0) Player.VelocityX = Math.Min(0, Player.VelocityX + Player.Friction);
            }

            Player.X += Player.VelocityX;
            ClampPlayerToCanvas();

            // Bullet hit enemy collisions
            foreach (var bullet in Bullets)
            {
                foreach (var enemy in Enemies)
                {
                    if (IsColliding(bullet.X, bullet.Y, 5, enemy.X, enemy.Y, enemy.Radius))
                    {
                        // neew to collect removals otherwise you get iterating errors
                        bulletsToRemove.Add(bullet);
                        enemiesToRemove.Add(enemy);

                        Score += 10;
                        KillPopups.Add(new KillConfirmed(enemy.X, enemy.Y, 10, 24, 24));
                        Explosions.Add(new Explosion(enemy.X, enemy.Y));
                        break;
                    }
                }
            }

            foreach (var bullet in bulletsToRemove)
                Bullets.Remove(bullet);

            foreach (var enemy in enemiesToRemove)
                Enemies.Remove(enemy);

            // Advance the enemy list 
            foreach (var enemy in Enemies.ToList())
                enemy.Update();

            // Advance the explosion and then remove anyone that is finished
            foreach (var exp in Explosions.ToList())
            {
                exp.Update();
                if (exp.IsDone) Explosions.Remove(exp);
            }

            foreach (var kc in KillPopups.ToList())
            {
                kc.Update();
                if (kc.IsDone) KillPopups.Remove(kc);
            }

            // Enemies that made it past the player 
            // Lose live and then remove the enemy from colelction
            var escapedEnemies = new List<Enemy>();
            foreach (var enemy in Enemies)
            {
                if (enemy.Y > Player.Y + Player.Radius + 10)
                {
                    Player.Lives--;
                    escapedEnemies.Add(enemy);
                }
            }
            foreach (var enemy in escapedEnemies)
                Enemies.Remove(enemy);

            // Enemy Player collide => lose a life and remove the enemy

            foreach (var enemy in Enemies.ToList())
            {
                float dx = Player.X - enemy.X;
                float dy = Player.Y - enemy.Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                if (distance < enemy.Radius + Player.Radius)
                {
                    Player.Lives--;
                    Enemies.Remove(enemy);
                }
            }


            // Spanw changes by difficulty set in the settings page
            _framesInBetweenSpawns++;
            if (_framesInBetweenSpawns >= _dynamicSpawnFrames)
            {
                _framesInBetweenSpawns = 0;

                float r = 12f;
                float minX = PlayPad + r;
                float maxX = CanvasWidth - PlayPad - r;

                if (maxX > minX)
                {
                    float startX = Random.Shared.NextSingle() * (maxX - minX) + minX;
                    float startY = PlayPad + r;

                    var enemy = new Enemy(startX, startY);
                    enemy.ApplyDifficulty(Preferences.Get(nameof(AppSettings.Difficulty), "Normal"));
                    // Use the speed modifier on spawn
                    enemy.Speed *= _speedMultiplier;
                    Enemies.Add(enemy);
                }
            }

            if (Player.Lives <= 0)
            {
                IsGameOver = true;
            }

            ClampPlayerToCanvas();
        }

        // Reset to a fresh run. Anything that changes value her will be reset to its default value
        public void Reset()
        {
            Score = 0;
            Player.Lives = 3;

            Player.X = CanvasWidth / 2f;
            SetPlayerAtBottom();

            Bullets.Clear();
            Enemies.Clear();
            KillPopups.Clear();
            IsGameOver = false;

            _lastUpdateUtc = DateTime.MinValue;
            _elapsedSecondsTotal = 0;
            _framesInBetweenSpawns = 0;
            _dynamicSpawnFrames = GetSpawnRateFromDifficulty();
            _speedMultiplier = 1f;
            _enemySoftCap = BaseEnemyCap;
        }

        // Maps difficulty string fed in from preference to a spawn interval in frames
        private int GetSpawnRateFromDifficulty()
        {
            var diff = Preferences.Get(nameof(AppSettings.Difficulty), "Normal");
            return diff switch
            {
                "Easy" => 150,
                "Normal" => 120,
                "Hard" => 80,
                _ => 120
            };
        }

        // Not using this in Update(), removed because of other errors for Debug
        //private float GetEnemySpeedMultiplier()
        //{
        //    var diff = Preferences.Get(nameof(AppSettings.Difficulty), "Normal");
        //    return diff switch
        //    {
        //        "Easy" => 0.7f,
        //        "Normal" => 1f,
        //        "Hard" => 1.4f,
        //        _ => 1f
        //    };
        //}


        // Keep the player inside the horizontal play area
        // Guards against early layout where min can exceed max where CanvasWidth wasn't ready for values
        public void ClampPlayerToCanvas()
        {
            float min = PlayPad + Player.Radius;
            float max = CanvasWidth - PlayPad - Player.Radius;

            
            if (CanvasWidth <= 0 || max <= min)
            {
                
                Player.X = CanvasWidth > 0 ? CanvasWidth * 0.5f : min;
                return;
            }
            Player.X = Math.Clamp(Player.X, PlayPad + Player.Radius, CanvasWidth - PlayPad - Player.Radius);
        }


        // Circle / Circle collisions using sqwuared distance
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
