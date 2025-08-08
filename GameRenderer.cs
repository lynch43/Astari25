using Astari25.Models;
using System.Collections.ObjectModel;

namespace Astari25.Views { 
   public class GameRenderer : IDrawable
    {

        
        private readonly Player _player;
        
        private readonly ObservableCollection<Bullet> _bullets;
        private readonly ObservableCollection<Enemy> _enemies;

        private readonly Func<float> _getCanvasWidth;

        public GameRenderer(Player player, ObservableCollection<Bullet> bullets, ObservableCollection<Enemy> enemies, Func<float> getCanvasWidth) {

            _player = player;
            _bullets = bullets;
            _enemies = enemies ?? new ObservableCollection<Enemy>();
            _getCanvasWidth = getCanvasWidth ?? throw new ArgumentNullException(nameof(getCanvasWidth));
        }

        
        
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {

            var canvasWidth = _getCanvasWidth();

            canvas.FillColor = Colors.Black;
            canvas.FillRectangle(dirtyRect); // Clear background

            canvas.StrokeColor = Colors.Purple;
            canvas.StrokeSize = 2;
            //canvas.DrawCircle(_player.X, _player.Y, _player.Radius);

            float clampedX = Math.Clamp(_player.X, _player.Radius, dirtyRect.Width - _player.Radius);
            canvas.DrawCircle(_player.X, _player.Y, _player.Radius);

            canvas.FillColor = Colors.Yellow;
            foreach (var bullet in _bullets.ToList()) {
                canvas.FillCircle(bullet.X, bullet.Y, 5);
            }

            canvas.StrokeColor = Colors.DarkRed;
            canvas.StrokeSize = 1;
            foreach (var enemy in _enemies.ToList()) {
                canvas.FillColor = Colors.Red;
                canvas.FillCircle(enemy.X, enemy.Y, 12);
                canvas.DrawCircle(enemy.X, enemy.Y, 12);
            }

            float r = 30f;
            //Console.WriteLine($"CANVAS SIZE: {dirtyRect.Width} x {dirtyRect.Height}");
            canvas.StrokeColor = Colors.DarkCyan;
            canvas.DrawRectangle(r, 0, canvasWidth - 2 * r, dirtyRect.Height);

        }
    }
}
