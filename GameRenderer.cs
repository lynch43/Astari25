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
            canvas.FillColor = Colors.Black;
            canvas.FillRectangle(dirtyRect); // Clear background

            
            float padding = 20f;
            float left = padding;
            float top = padding;
            float right = _getCanvasWidth() - padding;
            float bottom = dirtyRect.Height - padding;

            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 2;
            canvas.DrawRectangle(left, top, right - left, bottom - top);

            // ME
            canvas.FillColor = Colors.White;
            canvas.FillCircle(_player.X, _player.Y, _player.Radius);

            // PEW
            canvas.FillColor = Colors.Yellow;
            foreach (var bullet in _bullets.ToList())
            {
                canvas.FillCircle(bullet.X, bullet.Y, 5);
            }

            // Bad guy
            canvas.FillColor = Colors.Red;
            foreach (var enemy in _enemies.ToList())
            {
                canvas.FillCircle(enemy.X, enemy.Y, 12);
            }
        }
    }
}
