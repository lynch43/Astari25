using Microsoft.Maui.Graphics;
using Astari25.Models;

namespace Astari25.Views { 
   public class GameRenderer : IDrawable
    {

        // immutable field - ensure data remaining constant
        private readonly Player _player;

        public GameRenderer(Player player) {

            _player = player;
        }

        
        
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.Black;
            canvas.FillRectangle(dirtyRect); // Clear background

            canvas.StrokeColor = Colors.White;
            canvas.DrawCircle(_player.X, _player.Y, _player.Radius);
        }
    }
}
