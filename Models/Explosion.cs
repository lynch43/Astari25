// File used for:
// - A visual effect that happens when an enemy is destroyed
// - Get is half life from the frames and then the renderer makes it fade out
// - Esposes the isDone so the game loop can remove dinished effects

namespace Astari25.Models
{
    public class Explosion
    {

        // where to render the effect
        public float X { get; }
        public float Y { get; }

        // Frames remaining before the effect is finished
        public int framesLeft { get; private set; }

        // Total frames that the effect stays for. Render takes this for the erase
        public int totalFrames { get; }

        // Make a new explosion at x and y
        // lifeframes is how long it lasts.
        // messed around with it but it lasts roughly 0.3 sec right now
        public Explosion(float x, float y, int lifeFrames = 18)
        {
            X = x;
            Y = y;
            totalFrames = lifeFrames;
            framesLeft = lifeFrames;
        }


        // Ticks down one frame 
        // called every frame, or every time Update() is called
        public void Update() => framesLeft--;

        // This is true when the effect is getting removed from the collection.
        public bool IsDone => framesLeft <= 0;
    }
}