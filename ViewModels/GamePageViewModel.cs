using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Astari25.Views;
using Astari25.Models;

namespace Astari25.ViewModels
{
    public class GamePageViewModel
    {
        public Player Player { get; } = new Player();
        public IDrawable GameDrawable { get; }

        public GamePageViewModel() {
            GameDrawable = new GameRenderer(Player);
        }

        public void Update() {
            // go right slow
            //Player.X+=2;
        }
    }


}
