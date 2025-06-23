using Microsoft.Maui.Graphics;

public class GameRenderer : IDrawable {
    public void Draw(ICanvas canvas, RectF dirtyRect) {
        canvas.FillColor = Colors.Black;
        canvas.FillRectangle(dirtyRect); // Cleart background

        canvas.StrokeColor = Colors.White;
        canvas.DrawCircle(300, 300, 20); // IPlaceholder: Ship
    }
}