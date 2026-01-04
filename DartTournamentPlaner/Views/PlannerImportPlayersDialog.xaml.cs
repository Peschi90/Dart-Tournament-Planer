using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views
{
    public partial class PlannerImportPlayersDialog : Window, INotifyPropertyChanged
    {
        public ObservableCollection<PlayerSelectionRow> Players { get; } = new();
        public ObservableCollection<string> ClassOptions { get; } = new()
        {
            "Platin",
            "Gold",
            "Silber",
            "Bronze"
        };
 
        public IEnumerable<PlayerSelectionRow> PlayerSelections => Players;
 
        private readonly LocalizationService? _localizationService;
        private readonly PlannerTournamentSummary _summary;
        private readonly TournamentManagementService _tournamentService;
 
        public PlannerImportPlayersDialog(PlannerTournamentSummary summary, IEnumerable<PlannerParticipant> participants, LocalizationService? localizationService, TournamentManagementService tournamentService)
         {
            _summary = summary ?? throw new ArgumentNullException(nameof(summary));
            _localizationService = localizationService;
            _tournamentService = tournamentService ?? throw new ArgumentNullException(nameof(tournamentService));
             InitializeComponent();
             DataContext = this;
 
             ApplyLocalization();
 
             var participantList = participants?.ToList() ?? new();
             Debug.WriteLine($"[PlannerImportPlayersDialog] Received participants: {participantList.Count}");
 
             // Preload existing participants into grid with default class and empty group
             foreach (var p in participantList)
             {
                 var first = p.FirstName;
                 var last = p.LastName;
                 var nickname = p.Nickname;
 
                 var selectedClass = !string.IsNullOrWhiteSpace(p.ClassName) ? p.ClassName : ClassOptions.FirstOrDefault();
                 var group = p.GroupName;
 
                 Debug.WriteLine($"[PlannerImport] Preload participant: first='{first}' last='{last}' nick='{nickname}' class='{selectedClass}' group='{group}' email='{p.Email}'");
 
                 Players.Add(new PlayerSelectionRow
                 {
                     FirstName = first,
                     Nickname = nickname,
                     LastName = last,
                     Email = p.Email,
                     SelectedClass = selectedClass,
                     GroupName = group
                 });
             }
 
            // Always provide an empty row for quick additions
            if (Players.Count == 0)
            {
                Players.Add(new PlayerSelectionRow
                {
                    SelectedClass = ClassOptions.FirstOrDefault()
                });
            }
 
            // Bind ComboBox column to ClassOptions (DataGridComboBoxColumn is not in visual tree)
            var classColumn = PlayersDataGrid.Columns.OfType<DataGridComboBoxColumn>().FirstOrDefault();
            if (classColumn != null)
            {
                classColumn.ItemsSource = ClassOptions;
            }
         }
 
        private void ApplyLocalization()
        {
            try
            {
                string L(string key, string fallback) => _localizationService?.GetString(key) ?? fallback;
 
                Title = L("PlannerImportDialogTitle", Title);
                HeaderTitle.Text = L("PlannerImportHeader", HeaderTitle.Text);
 
                if (FirstNameColumn != null) FirstNameColumn.Header = L("PlannerImportFirstName", "Vorname");
                if (NicknameColumn != null) NicknameColumn.Header = L("PlannerImportNickname", "Spitzname");
                if (LastNameColumn != null) LastNameColumn.Header = L("PlannerImportLastName", "Nachname");
                if (EmailColumn != null) EmailColumn.Header = L("PlannerImportEmail", "E-Mail");
                if (ClassColumn != null) ClassColumn.Header = L("PlannerImportClass", "Klasse");
                if (GroupColumn != null) GroupColumn.Header = L("PlannerImportGroup", "Gruppe");
 
                AddRowButton.Content = L("PlannerImportAddRow", "Zeile hinzufuegen");
                RemoveRowButton.Content = L("PlannerImportRemoveRow", "Zeile entfernen");
                CancelButtonBottom.Content = L("PlannerImportCancel", "Abbrechen");
                SaveButton.Content = L("PlannerImportNext", "Nächster Schritt");
                HeaderTitle.Text = L("PlannerImportHeader", HeaderTitle.Text);
                Title = L("PlannerImportDialogTitle", Title);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"?? [Localization] Error applying planner import translations: {ex.Message}");
            }
        }
 
        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            Players.Add(new PlayerSelectionRow
            {
                SelectedClass = ClassOptions.FirstOrDefault()
            });
        }
 
        private void RemoveRow_Click(object sender, RoutedEventArgs e)
        {
            if (Players.Any())
            {
                Players.RemoveAt(Players.Count - 1);
            }
        }
 
        private void OkButton_Click(object sender, RoutedEventArgs e) => Close();
 
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
 
        private void NextStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentParticipants = Players.Select(p => new PlannerParticipant
                {
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Nickname = p.Nickname,
                    Email = p.Email,
                    ClassName = string.IsNullOrWhiteSpace(p.SelectedClass) ? ClassOptions.FirstOrDefault() : p.SelectedClass,
                    GroupName = p.GroupName
                }).ToList();
 
                var reviewDialog = new PlannerImportReviewDialog(_summary, currentParticipants, _tournamentService, _localizationService)
                {
                    Owner = this
                };
 
                var result = reviewDialog.ShowDialog();
                if (result == true)
                {
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error opening review dialog: {ex.Message}");
            }
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
 
        public class PlayerSelectionRow : INotifyPropertyChanged
        {
            private string? _firstName;
            private string? _nickname;
            private string? _lastName;
            private string? _email;
            private string? _selectedClass;
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
 
            public string? SelectedClass
            {
                get => _selectedClass;
                set { _selectedClass = value; OnPropertyChanged(); }
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
 }
