using System.IO;
using Microsoft.Maui.Storage;

// File used for: 
// - persisting a simple score history as plain text
// Every game get a line. Shoping time, score and difficulty
// - AppendAsync is called and then not used after a game

namespace Astari25.Services
{
    public static class TextResultService
    {
        // This is where the file lives:
        // AppDataDirectopry is the place where this data can live. maui does this
        
        static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "results.txt");

        // Add the One line result
        // utc - stored in utc to avoid issues
        // score and difficulty only other things to really show
        // could do more with this. like aleaderboard
        public static Task AppendAsync(DateTime utc, int score, string difficulty)
            => File.AppendAllTextAsync(
                FilePath,
                $"{utc:yyyy-MM-dd HH:mm:ss}Z | Score={score} | Difficulty={difficulty}{Environment.NewLine}"

            );


    }
}
