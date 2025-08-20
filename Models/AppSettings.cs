using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// File used for: 
// - Defines what app wide settings look like
// - Anything that is aved or loaded
// - Persistence is handled in the SettingsViewModel

namespace Astari25.Models
{
    public class AppSettings
    {
        // Difficulty for the game is Easy Medium or hard
        // Defaults to Normal. The settings page binds to this
        // and writes to preferences
        public string Difficulty { get; set; } = "Normal";
        
    }
}
