using Microsoft.Maui.Graphics;
namespace Astari25.Views { 
   public class GameRenderer : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.Black;
            canvas.FillRectangle(dirtyRect); // Clear background

            canvas.StrokeColor = Colors.White;
            canvas.FillCircle(300, 300, 20); // IPlaceholder: Ship
        }
    }
}
