// File used for:
// - ViewModel behind SettingsPage
// - binds Difficulty picker + Save/Reload buttons
// - Loads/saves AppSettings via SettingsService  Preferences + JSON
// - Exposes ICommand for Save/Reload
// - alert change notifications for UI

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Astari25.Models;
using Astari25.Services;
using Microsoft.Maui.Controls;

namespace Astari25.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public AppSettings Settings { get; private set; } = new();

        public ICommand SaveCommand { get; }
        public ICommand ReloadCommand { get; }

        public SettingsViewModel()
        {
            SaveCommand = new Command(SaveSettings);
            ReloadCommand = new Command(LoadSettings);
            LoadSettings();
        }

        private void LoadSettings()
        {
            var s = SettingsService.Load();

            // Reassign a fresh instance so bindings in UI update
            // does not implement INotifyPropertyChanged
            Settings = new AppSettings
            {
                Difficulty = s.Difficulty
            };

            OnPropertyChanged(nameof(Settings));
        }

        private void SaveSettings()
        {
            var s = SettingsService.Load();
            s.Difficulty = string.IsNullOrWhiteSpace(Settings?.Difficulty)
                ? "Normal"
                : Settings.Difficulty;

            SettingsService.Save(s);

            // Do this after save incase something tries to access
            OnPropertyChanged(nameof(Settings));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}