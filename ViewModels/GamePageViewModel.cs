using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Astari25.Views;
using Astari25.Models;
using System.Collections.ObjectModel;

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
            foreach (var bullet in Bullets) {
                bullet.Update();
            }

            foreach (var enemy in Enemies) {
                enemy.Update();
            }

            // Spawns enemy randomly within the 60 frames so 4 seconds?
            if (Random.Shared.Next(0, 50) == 0) {
                float startX = Random.Shared.Next(50, 500); // give random X coordinate
                Enemies.Add(new Enemy(startX, 0)); // 0 is top of screen so spawn at top

                Console.WriteLine($"bad guy spawned at coord={startX}");
            }
        }
    }


}
