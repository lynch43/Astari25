using System.IO;
using Microsoft.Maui.Storage;

namespace Astari25.Services
{
    public static class TextResultService
    {
        static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "results.txt");

        public static Task AppendAsync(DateTime utc, int score, string difficulty)
            => File.AppendAllTextAsync(
                FilePath,
                $"{utc:yyyy-MM-dd HH:mm:ss}Z | Score={score} | Difficulty={difficulty}{Environment.NewLine}"

            );


    }
}
