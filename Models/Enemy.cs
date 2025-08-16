namespace Astari25.Models
{
    public class Enemy
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Speed { get; set; } = 1f;

        public float Radius { get; set; } = 15f;

        public Enemy(float startX, float startY)
        {
            X = startX;
            Y = startY;
        }

        public void Update()
        {
            // move enemy down game screen
            Y += 2f * Speed;
        }

        public void ApplyDifficulty(string difficulty)
        {
            Speed = difficulty switch
            {
                "Easy" => 0.5f,
                "Normal" => 1f,
                "Hard" => 1.5f,
                _ => 1f
            };
        }
    }
}