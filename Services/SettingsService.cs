// File used for:
// - Centralized persistence for game settings like difficulty
// - Uses MAUI preferences to store data across different operating systems
//
// - Provides a tiny API to Load/Save settings for the app
// - Makes a preference key so existing calls
//   like Preferences.Get(nameof(AppSettings.Difficulty), "Normal") still work
// - If nothing is stored yet, there is a default value
//
// This class is small and has very little dependencies

using Astari25.Models;
using Microsoft.Maui.Storage;
using System.Text.Json;

namespace Astari25.Services
{
    public static class SettingsService
    {
        // Preference keys
        private const string JsonKey = "app_settings_json";
        private const string DifficultyKey = nameof(AppSettings.Difficulty);

        // JSON options
        private static readonly JsonSerializerOptions _json =
            new(JsonSerializerDefaults.General);

        // Returns a fully populated settings object 
        // First try and take from storage.
        // if not go to default values
        public static AppSettings Load()
        {
            // Prefer JSON if present
            var json = Preferences.Get(JsonKey, null);
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    var fromJson = JsonSerializer.Deserialize<AppSettings>(json, _json);
                    if (fromJson != null)
                        return Normalize(fromJson);
                }
                catch
                {
                    // Something wrong. or Corrupted. Go to defaults
                }
            }

            // Can be called from somewhere else
            var diff = Preferences.Get(DifficultyKey, "Normal");
            return Normalize(new AppSettings { Difficulty = diff });
        }

        // Saves settings to both JSON and keys used elsewhere
        public static void Save(AppSettings settings)
        {
            if (settings == null)
                return;

            // Persist JSON
            var json = JsonSerializer.Serialize(settings, _json);
            Preferences.Set(JsonKey, json);

            // copy difficulty
            var difficulty = string.IsNullOrWhiteSpace(settings.Difficulty) ? "Normal" : settings.Difficulty;
            Preferences.Set(DifficultyKey, difficulty);
        }

        // No need to hold the whole object. this for quick read or write
        public static string GetDifficulty()
            => Preferences.Get(DifficultyKey, "Normal");

        public static void SetDifficulty(string value)
        {
            var v = string.IsNullOrWhiteSpace(value) ? "Normal" : value;
            Preferences.Set(DifficultyKey, v);

            // Keep JSON in sync
            var s = Load();
            s.Difficulty = v;
            Save(s);
        }

        // Clears stored settings
        public static void Reset()
        {
            Preferences.Remove(JsonKey);
            Preferences.Remove(DifficultyKey);
        }

        // Ensures fields are non-null / clamped to valid values
        private static AppSettings Normalize(AppSettings s)
        {
            s.Difficulty = NormalizeDifficulty(s.Difficulty);
            return s;
        }

        private static string NormalizeDifficulty(string? value)
        {
            return value switch
            {
                "Easy" => "Easy",
                "Normal" => "Normal",
                "Hard" => "Hard",
                _ => "Normal"
            };
        }
    }
}