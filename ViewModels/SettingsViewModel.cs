using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Astari25.Models;

namespace Astari25.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public AppSettings Settings { get; set; } = new AppSettings();

        public ICommand SaveCommand { get; }
        public ICommand ReloadCommand { get; }

        public SettingsViewModel()
        {
            SaveCommand = new Command(SaveSettings);
            ReloadCommand = new Command(ReloadSettings);
        }

        private void SaveSettings()
        {
            // For now, just log or debug output.
            Console.WriteLine($"Settings saved: Difficulty = {Settings.Difficulty}");
        }

        private void ReloadSettings()
        {
            // For now, just reset to default
            Settings = new AppSettings();
            OnPropertyChanged(nameof(Settings));
            Console.WriteLine("Settings reloaded.");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}