using Microsoft.Maui.Graphics;
using Astari25.Models;
using System.Collections.ObjectModel;

namespace Astari25.Views { 
   public class GameRenderer : IDrawable
    {

        
        private readonly Player _player;
        private readonly ObservableCollection<Bullet> _bullets;

        public GameRenderer(Player player, ObservableCollection<Bullet> bullets) {

            _player = player;
            _bullets = bullets;
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
        }
    }
}
