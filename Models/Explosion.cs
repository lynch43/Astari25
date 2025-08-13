namespace Astari25.Models
{
    public class Explosion
    {
        public float X { get; }
        public float Y { get; }
        public int framesLeft { get; private set; }
        public int totalFrames { get; }

        public Explosion(float x, float y, int lifeFrames = 18)
        {
            X = x;
            Y = y;
            totalFrames = lifeFrames;
            framesLeft = lifeFrames;
        }

        public void Update() => framesLeft--;
        public bool IsDone => framesLeft <= 0;
    }
}