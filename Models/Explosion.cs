using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Astari25.Models
{
    
    internal class Explosion
    {
        public float X { get; }
        public float Y { get; }
        public int framesLeft { get; set; }
        public int totalFrames { get; }

        public Explosion(float x, float y, int lifeFrames = 18) {
            X = x; 
            Y = y;
            totalFrames = lifeFrames;
            framesLeft = lifeFrames;

        }

        public void Update() => framesLeft--;
            public bool IsDone => framesLeft <= 0;
        }
    }
}
