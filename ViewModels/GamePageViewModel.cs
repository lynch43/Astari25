using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Astari25.Views;
using Astari25.Models;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Astari25.ViewModels
{
    public class GamePageViewModel
    {
        public Player Player { get; } = new Player();
        public IDrawable GameDrawable { get; }

        public ObservableCollection<Bullet> Bullets { get; } = new ObservableCollection<Bullet>();

        public ObservableCollection<Enemy> Enemies { get; } = new ObservableCollection<Enemy>();

        public GamePageViewModel() {
            GameDrawable = new GameRenderer(Player, Bullets, Enemies);
        }

        public void Update() {
            // go right slow
            //Player.X+=2;

            // updat ethe collection
            foreach (var bullet in Bullets.ToList()) {
                bullet.Update();
            }

            foreach (var enemy in Enemies.ToList()) {
                enemy.Update();
            }

            foreach (var bullet in Bullets.ToList()) {

                foreach (var enemy in Enemies.ToList()) {
                    float dx = bullet.X - enemy.X;
                    float dy = bullet.Y - enemy.Y;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (distance < enemy.Radius + Player.Radius) // destrooy when they overlap
                    {
                        Console.WriteLine("Player hit, lose a life");
                        Player.Lives--;

                        Enemies.Remove(enemy);
                        Bullets.Remove(bullet);

                        if (Player.Lives <= 0) {
                            Console.WriteLine("Out of lives");
                            // have to add some more stuff here. game reset. message to player
                        }
                    }
                }
            
            }

            // Spawns enemy randomly within the 60 frames so 4 seconds?
            if (Random.Shared.Next(0, 120) == 0) {
                float startX = Random.Shared.Next(50, 600); // give random X coordinate
                Enemies.Add(new Enemy(startX, 0)); // 0 is top of screen so spawn at top

                Console.WriteLine($"bad guy spawned at coord={startX}"); // remove console line before sumission
            }

            
        }

        private bool IsColliding(float x1, float y1, float r1, float x2, float y2, float r2)
        {
            float dx = x1 - x2;
            float dy = y1 - y2;
            float distanceSquared = dx * dx * dy * dy;
            float radiusSum = r1 + r2;

            return distanceSquared <= radiusSum * radiusSum;

        }


    }


}
