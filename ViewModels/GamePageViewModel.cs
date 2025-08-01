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
            GameDrawable = new GameRenderer(Player, Bullets);
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
        }
    }


}
