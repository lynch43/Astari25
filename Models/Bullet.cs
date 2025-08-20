using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Astari25.Models
{
    public class Bullet
    {

        public float X { get; set; }
        public float Y { get; set; }
        public float Speed { get; set; } = 10;

        public Bullet(float startX, float startY) {
            X = startX;
            Y = startY;
        }

        public void Update() {
            Y -= Speed; // bullet go up
        }
    }
}
