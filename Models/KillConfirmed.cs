using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astari25.Models
{
    public class KillConfirmed
    {
        public float X { get; }
        public float Y { get; }
        public int hitPoint { get; }
        public int framesLeft { get; private set; }
        public int totalFrames { get; }

        public KillConfirmed(float x, float y, int hitPoint, int framesLeft, int totalFrames)
        {
            X = x;
            Y = y;
            this.hitPoint = hitPoint;
            this.framesLeft = framesLeft;
            this.totalFrames = totalFrames;
        } 
    }
}
