using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astari25.Models
{
    public class Player
    {

        public float X { get; set; } = 300;
        public float Y { get; set; } = 300;
        public float Radius { get; set; } = 20;

        public int Lives { get; set; } = 3;
    }
}
