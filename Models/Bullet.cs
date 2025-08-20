using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// File used for
// - this is a single bullet fired by the plater
// - Store position and movement speed
// - Update() advances the bullet upward every frame

namespace Astari25.Models
{
    public class Bullet
    {

        // Centre of bullet. will be pixels
        public float X { get; set; }
        public float Y { get; set; }

        // How many for every frame that it goes up
        public float Speed { get; set; } = 10f;

        // Spawn a bullet at a given starting position. player
        public Bullet(float startX, float startY) {
            X = startX;
            Y = startY;
        }


        // Move the bullet for a frame,
        // - the value make bullet go up 
        public void Update() {
            Y -= Speed;
        }
    }
}
