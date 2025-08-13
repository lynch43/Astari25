using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Graphics;
using Astari25.Models;

namespace Astari25.Views
{
    public class GameRenderer : IDrawable
    {
        private readonly Player _player;
        private readonly ObservableCollection<Bullet> _bullets;
        private readonly ObservableCollection<Enemy> _enemies;
        private readonly ObservableCollection<Explosion> _explosions;

        public GameRenderer(Player player,
                            ObservableCollection<Bullet> bullets,
                            ObservableCollection<Enemy> enemies,
                            ObservableCollection<Explosion> explosions)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _bullets = bullets ?? new ObservableCollection<Bullet>();
            _enemies = enemies ?? new ObservableCollection<Enemy>();
            _explosions = explosions ?? new ObservableCollection<Explosion>();
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            
            canvas.FillColor = Colors.Black;
            canvas.FillRectangle(dirtyRect);

            float padding = 20f;
            float left = padding;
            float top = padding;
            float right = dirtyRect.Width - padding;
            float bottom = dirtyRect.Height - padding;

            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 2;
            canvas.DrawRectangle(left, top, right - left, bottom - top);

           
            canvas.FillColor = Colors.White;
            canvas.FillCircle(_player.X, _player.Y, _player.Radius);

           
            canvas.FillColor = Colors.Yellow;
            foreach (var b in _bullets.ToList())
                canvas.FillCircle(b.X, b.Y, 5);

            canvas.FillColor = Colors.Red;
            foreach (var e in _enemies.ToList())
                canvas.FillCircle(e.X, e.Y, 12);

            foreach (var ex in _explosions.ToList())
            {
                float t = 1f - (ex.totalFrames / (float)ex.totalFrames);
                float radius = 8f + 32f * t;
                float alpha = 1f - t;

                canvas.FillColor = Colors.Orange.WithAlpha(0.25f * alpha);
                canvas.FillCircle(ex.X, ex.Y, radius * 0.55f);

                canvas.StrokeColor = Colors.Yellow.WithAlpha(0.8f * alpha);
                canvas.StrokeSize = 2;
                canvas.DrawCircle(ex.X, ex.Y, radius);
            }
        }
    }
}