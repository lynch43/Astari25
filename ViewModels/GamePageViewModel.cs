using Astari25.Views;
using Astari25.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Storage;

namespace Astari25.ViewModels
{
    public class GamePageViewModel : INotifyPropertyChanged
    {
        public Player Player { get; } = new Player();
        public IDrawable GameDrawable { get; }

        private int _framesInBetweenSpawns;
        private int FramesPerSpawn => GetSpawnRateFromDifficulty();

        public float CanvasWidth { get; set; } = 300f;
        public float CanvasHeight { get; set; } = 300f;

        public int MaxBulletsOnScreen { get; set; } = 5;
        TimeSpan _fireCooldown = TimeSpan.FromMilliseconds(180);
        DateTime _nextShotAtUtc = DateTime.MinValue;

        public ObservableCollection<Bullet> Bullets { get; } = new ObservableCollection<Bullet>();
        public ObservableCollection<Enemy> Enemies { get; } = new ObservableCollection<Enemy>();
        public ObservableCollection<Explosion> Explosions { get; } = new();
        public ObservableCollection<KillConfirmed> KillPopups { get; } = new();

        public GamePageViewModel()
        {
            GameDrawable = new GameRenderer(Player, Bullets, Enemies, Explosions, KillPopups, PlayPad);
        }

        public float PlayPad { get; set; } = 24f;

        public void SetPlayerAtBottom()
        {
            float bottomPadding = 35f;
            Player.Y = CanvasHeight - bottomPadding - Player.Radius;
        }

        public bool TryShoot(DateTime nowUtc)
        {
            if (nowUtc < _nextShotAtUtc) return false;

            PruneBullets();
            if (Bullets.Count >= MaxBulletsOnScreen) return false;

            Bullets.Add(new Bullet(Player.X, Player.Y));
            _nextShotAtUtc = nowUtc + _fireCooldown;
            return true;
        }

        private void PruneBullets()
        {
            for (int i = Bullets.Count - 1; i >= 0; i--)
            {
                if (Bullets[i].Y < -20)
                    Bullets.RemoveAt(i);
            }
        }

        public bool IsGameOver { get; set; } = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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

        public void Update()
        {
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

            Player.VelocityX += Player.InputX * Player.Acceleration;
            Player.VelocityX = Math.Clamp(Player.VelocityX, -Player.MaxSpeed, Player.MaxSpeed);

            if (Math.Abs(Player.InputX) < Player.DeadZone)
            {
                if (Player.VelocityX > 0) Player.VelocityX = Math.Max(0, Player.VelocityX - Player.Friction);
                else if (Player.VelocityX < 0) Player.VelocityX = Math.Min(0, Player.VelocityX + Player.Friction);
            }

            Player.X += Player.VelocityX;
            ClampPlayerToCanvas();

            foreach (var bullet in Bullets)
            {
                foreach (var enemy in Enemies)
                {
                    if (IsColliding(bullet.X, bullet.Y, 5, enemy.X, enemy.Y, enemy.Radius))
                    {
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

            foreach (var enemy in Enemies.ToList())
                enemy.Update();

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
        }

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
