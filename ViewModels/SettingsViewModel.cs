using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Astari25.Models;

namespace Astari25.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public AppSettings Settings { get; set; } = new();

        public ICommand SaveCommand { get; }
        public ICommand ReloadCommand { get; }

        public SettingsViewModel()
        {
            LoadSettings();

            SaveCommand = new Command(SaveSettings);
            ReloadCommand = new Command(LoadSettings);
        }

        private void LoadSettings()
        {
            Settings.Difficulty = Preferences.Get(nameof(Settings.Difficulty), "Normal");
            OnPropertyChanged(nameof(Settings));
        }

        private void SaveSettings()
        {
            Preferences.Set(nameof(Settings.Difficulty), Settings.Difficulty);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}