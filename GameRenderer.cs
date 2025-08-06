using Astari25.Models;
using System.Collections.ObjectModel;

namespace Astari25.Views { 
   public class GameRenderer : IDrawable
    {

        
        private readonly Player _player;
        
        private readonly ObservableCollection<Bullet> _bullets;
        private readonly ObservableCollection<Enemy> _enemies;

        public GameRenderer(Player player, ObservableCollection<Bullet> bullets, ObservableCollection<Enemy> enemies) {

            _player = player;
            _bullets = bullets;
            _enemies = enemies ?? new ObservableCollection<Enemy>();
        }

        
        
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.Black;
            canvas.FillRectangle(dirtyRect); // Clear background

            canvas.StrokeColor = Colors.White;
            canvas.DrawCircle(_player.X, _player.Y, _player.Radius);

            canvas.FillColor = Colors.Yellow;
            foreach (var bullet in _bullets) {
                canvas.FillCircle(bullet.X, bullet.Y, 5);
            }

            canvas.FillColor = Colors.Red;
            foreach (var enemy in _enemies) {
                canvas.FillCircle(enemy.X, enemy.Y, 12);
            }

        }
    }
}
