// File used for:
// - IDrawable is made here
// - Popups also rendered here from the View Model Data
// - Draw Method here that dictates what all entities on screen look like

// - Readonly. no game rules or anything. It pulls state from the View Model collections
// - All the entities get painted over the ICanvas ( Player, poups etc. )
// - Everything needs to be called every frame in the timer so it needs to be accessible during all logic
// 

// - Created once every call of GamePageViewModel
// - View Model creates it and passes its own data
// - GraphicsView calls Draw() every time that Invalidate() from the game loop

// - All Collections get changed only on the UI thread by the View Model
// - This class only reads a screenshot of the information. Avoids enumeration issues. 
// - Everything is using .ToList() to fix the enumeration problem

// Layout:
// - ( 0,0 ) is top left of GraphicsView
// - _playPaf adds a visual + logical margin around the play area
// - Player.X/Y etc. are already in view coordinates
// View Model we know has correct info all the time

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Graphics;
using Astari25.Models;

namespace Astari25.Views
{
    public class GameRenderer : IDrawable
    {
        // Information here is owned by View Model
        // - _player: single ship shaped as triangle
        // - _bullets: yellow sicrles that travel up
        // - _enemies: red cirles that travel down
        // - _explosions: circle that stays still where enemies die
        // - _killPopups: floating score notifier text near the hit
        // - _playPad: padding used as a visual margin. also dictates where border goes

        private readonly Player _player;
        private readonly ObservableCollection<Bullet> _bullets;
        private readonly ObservableCollection<Enemy> _enemies;
        private readonly ObservableCollection<Explosion> _explosions;
        private readonly ObservableCollection<KillConfirmed> _killPopups;
        private readonly float _playPad;

        // Constructor
        // - player cannot be null
        // - collisions can be null.
        // - collections left empty by default so it doesn't mess with Draw()
        public GameRenderer(Player player,
                            ObservableCollection<Bullet> bullets,
                            ObservableCollection<Enemy> enemies,
                            ObservableCollection<Explosion> explosions,
                            ObservableCollection<KillConfirmed> killPopups,
                            float playPad)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _bullets = bullets ?? new ObservableCollection<Bullet>();
            _enemies = enemies ?? new ObservableCollection<Enemy>();
            _explosions = explosions ?? new ObservableCollection<Explosion>();
            _killPopups = killPopups ?? new();
            _playPad = playPad;
        }


        // Draw Order ( z index from back to front )
        // 1) Clear to black color
        // 2) Play area border ( white triangle inside the padding )
        // 3) Player ship
        // 4) Bullets ( yellow ) enemies ( red )
        // 5) Explosions ( rings after hit )
        // 6) Kill popups ( score comes up and then fades )
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            
            // Background color , covering everywhere
            canvas.FillColor = Colors.Black;
            canvas.FillRectangle(dirtyRect);

            // _playPad is the margin for the inner Game area
            float left = _playPad;
            float top = _playPad;
            float right = dirtyRect.Width - _playPad;
            float bottom = dirtyRect.Height - _playPad;


            // visual border. play are boundary
            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 2;
            canvas.DrawRectangle(left, top, right - left, bottom - top);


            // Player ship , triangle based on player radius for the size
            canvas.FillColor = Colors.White;
            var shipSize = _player.Radius * 2;
            PathF playerPath = new PathF();
            playerPath.MoveTo(_player.X, _player.Y - shipSize);                        
            playerPath.LineTo(_player.X - shipSize * 0.5f, _player.Y + shipSize * 0.6f); 
            playerPath.LineTo(_player.X + shipSize * 0.5f, _player.Y + shipSize * 0.6f);
            playerPath.Close();
            canvas.FillPath(playerPath);


            // Bullets .ToList() avoids collection modified exceptions mid calculation
            canvas.FillColor = Colors.Yellow;
            foreach (var b in _bullets.ToList())
                canvas.FillCircle(b.X, b.Y, 5);

            // Enemies ( red circles )
            canvas.FillColor = Colors.Red;
            foreach (var e in _enemies.ToList())
                canvas.FillCircle(e.X, e.Y, 12);


            // Explosions, expanding ring and then fade out divided by frames
            // The 't' is 0. 
            // totalFrames/totalFreames = 1 => ( 1- 1 ) = 0
            // I can change this and make animation on expansion. the ratio is remaining / elapsed
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


            // Kill confirmed pop u ps
            foreach (var kc in _killPopups.ToList())
            {
                // from 0 - 1 as the popup ages over time
                var t = 1f - kc.framesLeft / (float)kc.totalFrames; 
                canvas.FontSize = 32f;
                // fades out over time
                canvas.FontColor = Colors.Gold.WithAlpha(1f - t);
                // Centered text in a 140x70 rect
                // Moves upwards as 0 goes to 1 ( t )
                canvas.DrawString(
                    $"+{kc.hitPoint}",
                    new RectF(kc.X - 70, kc.Y - 35 - 40f * t, 140, 70),
                    HorizontalAlignment.Center, VerticalAlignment.Center
                );
            }
        }
    }
}