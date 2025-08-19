// File used for:
// - Represent a single enemy instance on the screen
// - Holds Positon x, y , movement speed and where circles collide using radius
// Update() advances the enemy downwards each frame
// AppleDifficulty() maps difficulty string variable

// View Model has a collection of enemy object and call Update() every tick.
// The renderer reads X / Y / Radius to draw circles
// Nothing changes in this class. everything happens on UI thread via the View Model



namespace Astari25.Models
{
    public class Enemy
    {
        // Positions used by renderer and collison logic
        public float X { get; set; }
        public float Y { get; set; }

        // Mopvement scale Update() uses this to make enemies go down
        public float Speed { get; set; } = 1f;

        // Collision radius also used for sizing in the renderer
        public float Radius { get; set; } = 15f;

        // Spawn with a starting position, View Model dictates that position
        public Enemy(float startX, float startY)
        {
            X = startX;
            Y = startY;
        }

        // Advance one every frame. Move downward by a constant step scaled with Speed
        public void Update()
        {
            
            Y += 2f * Speed;
        }


        // Set speed with the difficulty label in settings. 
        // IOf something goes wrong set it to same speed as Normal
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