using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astari25.Models
{

    public class GameResult
    {
        public DateTime TimestampUtc { get; set; }
        public int Score { get; set; }
        public string Difficulty { get; set; } = "Normal";

    }
}
