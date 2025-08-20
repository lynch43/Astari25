using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;
using Astari25.Models;

namespace Astari25.Services
{

    public static class ResultService
    {
        static readonly string FilePath =
            Path.Combine(FileSystem.AppDataDirectory, "result.json");

        public static async Task<List<GameResult>> LoadAsync()
        {
            try
            {
                if (!File.Exists(FilePath)) {
                    return new List<GameResult>();
                }
                
                using var stream = File.OpenRead(FilePath);
                var list = await JsonSerializer.DeserializeAsync<List<GameResult>>(stream) ?? new List<GameResult>();
                return list;
            }
            catch
            {
                return new List<GameResult>();
            }
        }

        public static async Task AppendAsync(GameResult result, int keep = 50)
        {
            var all = await LoadAsync();
            all.Insert(0, result);
            if (all.Count > keep) {
                all = all.Take(keep).ToList();
            } 

            Directory.CreateDirectory(FileSystem.AppDataDirectory);
            using var stream = File.Create(FilePath);
            await JsonSerializer.SerializeAsync(stream, all,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
