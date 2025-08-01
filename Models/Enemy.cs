using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astari25.Models
{
    class Enemy
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Speed { get; set; } = 2;

        public Enemy(float startX, float startY) {
            X = startX;
            Y = startY;
        }

        public void Update() {
            // move enemy down game screen spaceinvaders style for now
            Y -= Speed;
        }
    }
}
