using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using DartTournamentPlaner.Models.PowerScore;

namespace DartTournamentPlaner.Views;

public partial class PowerScoringAddPlayersDialog : Window, INotifyPropertyChanged
{
    public ObservableCollection<PlayerEntryRow> Players { get; } = new();
    public IEnumerable<PlayerEntryRow> PlayerEntries => Players;

    public PowerScoringAddPlayersDialog(IEnumerable<PowerScoringPlayer> existingPlayers)
    {
        System.Windows.Application.LoadComponent(this, new System.Uri("/DartTournamentPlaner;component/Views/PowerScoringAddPlayersDialog.xaml", System.UriKind.Relative));
        DataContext = this;

        foreach (var player in existingPlayers)
        {
            Players.Add(new PlayerEntryRow
            {
                Nickname = player.Name
            });
        }

        if (Players.Count == 0)
        {
            Players.Add(new PlayerEntryRow());
        }
        else
        {
            Players.Add(new PlayerEntryRow());
        }

        Debug.WriteLine($"[PowerScoringAddPlayersDialog] Loaded {Players.Count} rows (including empty)");
    }

    private void AddRow_Click(object sender, RoutedEventArgs e)
    {
        Players.Add(new PlayerEntryRow());
        Debug.WriteLine("[PowerScoringAddPlayersDialog] Added row");
    }

    private void RemoveRow_Click(object sender, RoutedEventArgs e)
    {
        if (Players.Count > 0)
        {
            Players.RemoveAt(Players.Count - 1);
            Debug.WriteLine("[PowerScoringAddPlayersDialog] Removed last row");
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class PlayerEntryRow : INotifyPropertyChanged
    {
        private string? _firstName;
        private string? _nickname;
        private string? _lastName;
        private string? _email;

        public string? FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(); }
        }

        public string? Nickname
        {
            get => _nickname;
            set { _nickname = value; OnPropertyChanged(); }
        }

        public string? LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); }
        }

        public string? Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
