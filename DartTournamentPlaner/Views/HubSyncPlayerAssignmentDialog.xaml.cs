using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DartTournamentPlaner.Views;

public partial class HubSyncPlayerAssignmentDialog : Window
{
    public ObservableCollection<AssignmentItem> Players { get; }
    public ObservableCollection<SummaryEntry> Summary { get; } = new();

    public HubSyncPlayerAssignmentDialog(IEnumerable<ParticipantDisplay> participants, IEnumerable<string> classOptions)
    {
        var classes = classOptions?.ToList() ?? new List<string> { "Platin", "Gold", "Silber", "Bronze" };
        Players = new ObservableCollection<AssignmentItem>(participants.Select(p => new AssignmentItem
        {
            Name = p.DisplayName,
            Average = p.Average,
            SelectedClass = classes.FirstOrDefault() ?? "Platin",
            SelectedGroup = "1",
            ClassOptions = classes
        }));

        DataContext = this;
        InitializeComponent();

        Players.CollectionChanged += (_, __) => RecalculateSummary();
        foreach (var item in Players) item.PropertyChanged += OnItemChanged;
        RecalculateSummary();
    }

    public IReadOnlyList<AssignmentItem> GetAssignments() => Players.ToList();

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AssignmentItem.SelectedClass) || e.PropertyName == nameof(AssignmentItem.SelectedGroup))
        {
            RecalculateSummary();
        }
    }

    private void RecalculateSummary()
    {
        var order = new List<string> { "Platin", "Gold", "Silber", "Bronze" };
        var grouped = Players
            .GroupBy(p => new { p.SelectedClass, p.SelectedGroup })
            .OrderBy(g => order.IndexOf(g.Key.SelectedClass))
            .ThenBy(g => g.Key.SelectedGroup)
            .Select(g => new SummaryEntry
            {
                Class = g.Key.SelectedClass,
                Group = g.Key.SelectedGroup,
                Count = g.Count()
            })
            .ToList();

        Summary.Clear();
        foreach (var entry in grouped)
        {
            Summary.Add(entry);
        }
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }
}

public class AssignmentItem : INotifyPropertyChanged
{
    private string _selectedClass = "Platin";
    private string _selectedGroup = "1";

    public string Name { get; set; } = string.Empty;
    public string SelectedClass
    {
        get => _selectedClass;
        set { _selectedClass = value; OnPropertyChanged(); }
    }
    public string SelectedGroup
    {
        get => _selectedGroup;
        set { _selectedGroup = value; OnPropertyChanged(); }
    }
    public double? Average { get; set; }
    public List<string> ClassOptions { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class ParticipantDisplay
{
    public string DisplayName { get; set; } = string.Empty;
    public double? Average { get; set; }
}

public class SummaryEntry
{
    public string Class { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public int Count { get; set; }
}
