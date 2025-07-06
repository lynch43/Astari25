using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Astari25.Views;

namespace Astari25.ViewModels
{
    internal class GamePageViewModel
    {
        public IDrawable GameDrawable { get; } = new GameRenderer();
    }
}
