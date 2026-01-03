using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace DartTournamentPlaner.Views;

public partial class AddPlayersDialog : Window, INotifyPropertyChanged
{
    public ObservableCollection<PlayerEntryRow> Players { get; } = new();
    public ObservableCollection<string> Groups { get; } = new();

    public IEnumerable<PlayerEntryRow> PlayerEntries => Players;

    private readonly LocalizationService _localizationService;

    public AddPlayersDialog(IEnumerable<Group> groups, Group? preselectedGroup, LocalizationService localizationService)
    {
        _localizationService = localizationService;
        InitializeComponent();
        DataContext = this;

        var groupList = groups.ToList();

        Debug.WriteLine($"[AddPlayersDialog] Received groups: {groupList.Count}");
        foreach (var g in groupList)
        {
            Debug.WriteLine($"[AddPlayersDialog] Group: {g.Name}, Players: {g.Players.Count}");
        }

        foreach (var g in groupList)
        {
            Groups.Add(g.Name);
        }

        // Vorhandene Spieler der Klasse vorbefüllen
        foreach (var entry in groupList.SelectMany(g => g.Players.Select(p => new { Player = p, GroupName = g.Name })))
        {
            Players.Add(new PlayerEntryRow
            {
                FirstName = entry.Player.FirstName,
                Nickname = entry.Player.Nickname,
                LastName = entry.Player.LastName,
                Email = entry.Player.Email,
                GroupName = entry.GroupName
            });
            Debug.WriteLine($"[AddPlayersDialog] Preload player: {entry.Player.Name} ({entry.Player.FirstName} {entry.Player.LastName}) Group={entry.GroupName}");
        }

        // Immer mindestens eine leere Zeile für neue Spieler bereitstellen
        var defaultGroup = preselectedGroup?.Name ?? Groups.FirstOrDefault();
        Debug.WriteLine($"[AddPlayersDialog] Default group for new rows: {defaultGroup ?? "<none>"}");
        if (Players.Count == 0)
        {
            Players.Add(new PlayerEntryRow { GroupName = defaultGroup });
            Debug.WriteLine("[AddPlayersDialog] Added empty row (no existing players)");
        }
        else
        {
            Players.Add(new PlayerEntryRow { GroupName = defaultGroup });
            Debug.WriteLine("[AddPlayersDialog] Added empty row (with existing players)");
        }

        // DataGrid ComboBox Column manuell mit Groups verbinden, damit ItemsSource sicher gesetzt wird
        var groupColumn = PlayersDataGrid.Columns.OfType<DataGridComboBoxColumn>().FirstOrDefault();
        if (groupColumn != null)
        {
            groupColumn.ItemsSource = Groups;
            Debug.WriteLine($"[AddPlayersDialog] Bound group column ItemsSource to Groups (count={Groups.Count})");
        }
    }

    private void AddRow_Click(object sender, RoutedEventArgs e)
    {
        var defaultGroup = Groups.FirstOrDefault();
        Players.Add(new PlayerEntryRow { GroupName = defaultGroup });
        Debug.WriteLine($"[AddPlayersDialog] Added row via button. Group default={defaultGroup ?? "<none>"}");
    }

    private void RemoveRow_Click(object sender, RoutedEventArgs e)
    {
        if (Players.Any())
        {
             Players.RemoveAt(Players.Count - 1);
            Debug.WriteLine("[AddPlayersDialog] Removed last row via button");
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
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
        private string? _groupName;

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

        public string? GroupName
        {
            get => _groupName;
            set { _groupName = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
