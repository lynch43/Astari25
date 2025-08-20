using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// File used for:
// - Represents a flaoting +10 when you kill something
// - Tracks where to render it and how long it should stay on screen
// - Update() counts down drames
// - IsDone tells the game loop when to remove it


namespace Astari25.Models
{
    public class KillConfirmed
    {

        // Where the pop up starts. enemy position at time of impact
        public float X { get; }
        public float Y { get; }

        // How many points to display ( right now it is 10 )
        public int hitPoint { get; }

        // Frames left before it goes away
        public int framesLeft { get; private set; }

        // The amount of Update() cycles that it should live for
        public int totalFrames { get; }


        // Create a pop up at x and y that shows hitpoint
        // and lives for framesLeft / totalFrames
        public KillConfirmed(float x, float y, int hitPoint, int framesLeft, int totalFrames)
        {
            X = x;
            Y = y;
            this.hitPoint = hitPoint;
            this.framesLeft = framesLeft;
            this.totalFrames = totalFrames;
        }


        // one tick of lifetime
        // this is called every frame from the game loop
        public void Update() => framesLeft--;

        // turns true when the pop up should be removed from collection
        public bool IsDone => framesLeft <= 0;
    }
}
