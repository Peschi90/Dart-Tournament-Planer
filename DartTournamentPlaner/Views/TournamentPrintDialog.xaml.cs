using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Helpers;

namespace DartTournamentPlaner.Views
{
    /// <summary>
    /// Dialog für die Auswahl und Vorschau von Turnier-Druckoptionen
    /// Ermöglicht die Konfiguration welche Teile des Turniers gedruckt werden sollen
    /// </summary>
    public partial class TournamentPrintDialog : Window
    {
        private readonly List<TournamentClass> _tournamentClasses;
        private TournamentClass _selectedTournamentClass;
        private readonly PrintService _printService;
        private readonly LocalizationService? _localizationService;
        private readonly List<CheckBox> _groupCheckBoxes = new List<CheckBox>();

        /// <summary>
        /// Die konfigurierten Druckoptionen
        /// </summary>
        public TournamentPrintOptions PrintOptions { get; private set; }

        /// <summary>
        /// Gibt an ob der Benutzer den Druckvorgang bestätigt hat
        /// </summary>
        public bool PrintConfirmed { get; private set; } = false;

        /// <summary>
        /// Konstruktor für den Druckdialog mit Klassenauswahl und HubService
        /// </summary>
        /// <param name="tournamentClasses">Alle verfügbaren Turnierklassen</param>
        /// <param name="selectedTournamentClass">Die initial ausgewählte Turnierklasse</param>
        /// <param name="localizationService">Service für Übersetzungen</param>
        /// <param name="hubService">Hub Service für QR-Code Generierung (optional)</param>
        /// <param name="tournamentId">Tournament-ID für QR-Code URLs (optional)</param>
        public TournamentPrintDialog(List<TournamentClass> tournamentClasses, TournamentClass? selectedTournamentClass = null, 
            LocalizationService? localizationService = null, HubIntegrationService? hubService = null, string? tournamentId = null)
        {
            InitializeComponent();
            
            _tournamentClasses = tournamentClasses ?? throw new ArgumentNullException(nameof(tournamentClasses));
            if (!_tournamentClasses.Any())
                throw new ArgumentException("At least one tournament class is required", nameof(tournamentClasses));
                
            _selectedTournamentClass = selectedTournamentClass ?? _tournamentClasses.First();
            _localizationService = localizationService;
    
         // ✅ FIXED: PrintService MIT HubService UND Tournament-ID initialisieren
         _printService = new PrintService(localizationService, hubService, tournamentId);
     System.Diagnostics.Debug.WriteLine($"[TournamentPrintDialog] PrintService initialized with HubService: {hubService != null}, TournamentId: {tournamentId ?? "null"}");
      
  PrintOptions = new TournamentPrintOptions();

    InitializeDialog();
            UpdatePreview();
        }

        /// <summary>
        /// Legacy-Konstruktor für Kompatibilität
        /// </summary>
        /// <param name="tournamentClass">Die zu druckende Turnierklasse</param>
        /// <param name="localizationService">Service für Übersetzungen</param>
        public TournamentPrintDialog(TournamentClass tournamentClass, LocalizationService? localizationService = null)
            : this(new List<TournamentClass> { tournamentClass }, tournamentClass, localizationService, null, null)
        {
        }

        /// <summary>
        /// Initialisiert den Dialog mit den verfügbaren Optionen
        /// </summary>
        private void InitializeDialog()
        {
            // Lokalisierung anwenden
            ApplyLocalization();
            
            // Turnierklassen-ComboBox erstellen
            CreateTournamentClassSelector();
            
            // Titel setzen
            UpdateTournamentClassLabel();
            
            // Gruppen-CheckBoxes erstellen
            CreateGroupCheckBoxes();
            
            // Verfügbarkeit der Optionen prüfen
            CheckOptionAvailability();
            
            // Event-Handler für Optionsänderungen hinzufügen
            AddOptionChangeHandlers();
        }

        /// <summary>
        /// Wendet die Lokalisierung auf alle UI-Elemente an
        /// </summary>
        private void ApplyLocalization()
        {
            try
            {
                // Fenstertitel
                Title = _localizationService?.GetString("PrintTournamentStatistics") ?? "Print Tournament Statistics";

                // Einfache Lokalisierung über Content-Property der vorhandenen Elemente
                if (IncludeOverviewCheckBox != null)
                    IncludeOverviewCheckBox.Content = _localizationService?.GetString("TournamentOverviewOption") ?? "Tournament Overview";

                if (IncludeGroupPhaseCheckBox != null)
                    IncludeGroupPhaseCheckBox.Content = _localizationService?.GetString("IncludeGroupPhase") ?? "Include Group Phase";

                if (SelectAllGroupsCheckBox != null)
                    SelectAllGroupsCheckBox.Content = _localizationService?.GetString("AllGroups") ?? "All Groups";

                if (IncludeFinalsCheckBox != null)
                    IncludeFinalsCheckBox.Content = _localizationService?.GetString("IncludeFinals") ?? "Include Finals";

                if (IncludeKnockoutCheckBox != null)
                    IncludeKnockoutCheckBox.Content = _localizationService?.GetString("IncludeKnockout") ?? "Include KO Phase";

                if (IncludeWinnerBracketCheckBox != null)
                    IncludeWinnerBracketCheckBox.Content = _localizationService?.GetString("WinnerBracket") ?? "Winner Bracket";

                if (IncludeLoserBracketCheckBox != null)
                    IncludeLoserBracketCheckBox.Content = _localizationService?.GetString("LoserBracket") ?? "Loser Bracket";

                if (IncludeKnockoutParticipantsCheckBox != null)
                    IncludeKnockoutParticipantsCheckBox.Content = _localizationService?.GetString("ParticipantsList") ?? "Participants List";

                // Buttons
                if (PreviewButton != null)
                    PreviewButton.Content = _localizationService?.GetString("UpdatePreview") ?? "👁️ Update Preview";

                if (PrintButton != null)
                    PrintButton.Content = _localizationService?.GetString("PrintButton") ?? "🖨️ Print";

                if (CancelButton != null)
                    CancelButton.Content = _localizationService?.GetString("CancelButton") ?? "❌ Cancel";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplyLocalization: ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Erstellt die Turnierklassen-Auswahl ComboBox
        /// </summary>
        private void CreateTournamentClassSelector()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("CreateTournamentClassSelector: Starting...");
                
                if (TournamentClassComboBox != null)
                {
                    TournamentClassComboBox.Items.Clear();
                    
                    foreach (var tournamentClass in _tournamentClasses)
                    {
                        var hasData = PrintHelper.HasPrintableContent(tournamentClass);
                        var displayName = hasData ? 
                            _localizationService?.GetString("ActiveTournamentClass", tournamentClass.Name) ?? $"🏆 {tournamentClass.Name}" : 
                            _localizationService?.GetString("EmptyTournamentClass", tournamentClass.Name) ?? $"⚪ {tournamentClass.Name} (empty)";
                            
                        var comboBoxItem = new ComboBoxItem
                        {
                            Content = displayName,
                            Tag = tournamentClass,
                            IsEnabled = hasData
                        };
                        
                        TournamentClassComboBox.Items.Add(comboBoxItem);
                        
                        if (tournamentClass == _selectedTournamentClass)
                        {
                            TournamentClassComboBox.SelectedItem = comboBoxItem;
                        }
                    }
                    
                    // Fallback: Erste verfügbare Klasse auswählen wenn aktuelle leer ist
                    if (TournamentClassComboBox.SelectedItem == null)
                    {
                        var firstAvailable = TournamentClassComboBox.Items.Cast<ComboBoxItem>()
                            .FirstOrDefault(item => item.IsEnabled);
                        if (firstAvailable != null)
                        {
                            TournamentClassComboBox.SelectedItem = firstAvailable;
                            _selectedTournamentClass = (TournamentClass)firstAvailable.Tag;
                        }
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"CreateTournamentClassSelector: Created {TournamentClassComboBox.Items.Count} items, selected: {_selectedTournamentClass.Name}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateTournamentClassSelector: ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Event-Handler für Turnierklassen-Auswahl
        /// </summary>
        private void TournamentClassComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    var newTournamentClass = (TournamentClass)selectedItem.Tag;
                    if (newTournamentClass != _selectedTournamentClass)
                    {
                        System.Diagnostics.Debug.WriteLine($"TournamentClassComboBox_SelectionChanged: Switching from {_selectedTournamentClass.Name} to {newTournamentClass.Name}");
                        
                        _selectedTournamentClass = newTournamentClass;
                        
                        // UI komplett aktualisieren
                        UpdateTournamentClassLabel();
                        CreateGroupCheckBoxes();
                        CheckOptionAvailability();
                        UpdatePreview();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TournamentClassComboBox_SelectionChanged: ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Aktualisiert das Turnierklassen-Label
        /// </summary>
        private void UpdateTournamentClassLabel()
        {
            if (TournamentClassLabel != null)
            {
                var groupCount = _selectedTournamentClass.Groups?.Count ?? 0;
                var playerCount = _selectedTournamentClass.Groups?.SelectMany(g => g.Players).Count() ?? 0;
                TournamentClassLabel.Text = _localizationService?.GetString("SelectTournamentClass", _selectedTournamentClass.Name, groupCount, playerCount) ?? $"Tournament Class: {_selectedTournamentClass.Name} ({groupCount} Groups, {playerCount} Players)";
            }
        }

        /// <summary>
        /// Erstellt CheckBoxes für alle verfügbaren Gruppen
        /// </summary>
        private void CreateGroupCheckBoxes()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"CreateGroupCheckBoxes: Starting for {_selectedTournamentClass.Name}");
                
                // Sichere Behandlung der UI-Controls
                if (IndividualGroupsPanel != null)
                {
                    IndividualGroupsPanel.Children.Clear();
                }
                _groupCheckBoxes.Clear();

                var groups = _selectedTournamentClass.Groups;
                if (groups == null || !groups.Any())
                {
                    System.Diagnostics.Debug.WriteLine("CreateGroupCheckBoxes: No groups available");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"CreateGroupCheckBoxes: Creating checkboxes for {groups.Count} groups");

                foreach (var group in groups)
                {
                    if (group == null) continue;
                    
                    var playersCount = group.Players?.Count ?? 0;
                    var groupText = _localizationService?.GetString("GroupWithPlayers", group.Name, playersCount) ?? $"{group.Name} ({playersCount} Players)";
                    var checkBox = new CheckBox
                    {
                        Content = groupText,
                        IsChecked = true,
                        Margin = new Thickness(0, 2, 0, 2),
                        Tag = group.Id
                    };
                    
                    checkBox.Checked += GroupCheckBox_Changed;
                    checkBox.Unchecked += GroupCheckBox_Changed;
                    
                    if (IndividualGroupsPanel != null)
                    {
                        IndividualGroupsPanel.Children.Add(checkBox);
                    }
                    _groupCheckBoxes.Add(checkBox);
                    
                    System.Diagnostics.Debug.WriteLine($"  Created checkbox for group: {group.Name} (ID: {group.Id})");
                }
                
                System.Diagnostics.Debug.WriteLine($"CreateGroupCheckBoxes: Successfully created {_groupCheckBoxes.Count} checkboxes");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateGroupCheckBoxes: ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"CreateGroupCheckBoxes: Stack trace: {ex.StackTrace}");
                
                // Sichere Bereinigung bei Fehler
                _groupCheckBoxes.Clear();
                if (IndividualGroupsPanel != null)
                {
                    IndividualGroupsPanel.Children.Clear();
                }
            }
        }

        /// <summary>
        /// Prüft welche Druckoptionen verfügbar sind und aktiviert/deaktiviert entsprechende Controls
        /// </summary>
        private void CheckOptionAvailability()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"CheckOptionAvailability: Checking availability for {_selectedTournamentClass.Name}");
                
                // Gruppenphase ist verfügbar wenn Gruppen vorhanden sind
                var hasGroups = _selectedTournamentClass.Groups?.Any() ?? false;
                System.Diagnostics.Debug.WriteLine($"CheckOptionAvailability: HasGroups = {hasGroups}, Groups count = {_selectedTournamentClass.Groups?.Count ?? 0}");

                // Sichere Behandlung der Gruppenphase-Controls
                if (IncludeGroupPhaseCheckBox != null)
                {
                    IncludeGroupPhaseCheckBox.IsEnabled = hasGroups;
                    if (!hasGroups)
                    {
                        IncludeGroupPhaseCheckBox.IsChecked = false;
                    }
                }

                if (GroupSelectionPanel != null)
                {
                    GroupSelectionPanel.IsEnabled = hasGroups;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("CheckOptionAvailability: WARNING - GroupSelectionPanel is null");
                }

                // Finalrunde ist verfügbar wenn in Finals-Phase oder bereits abgeschlossen
                var hasFinalsPhase = (_selectedTournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals) ||
                                   (_selectedTournamentClass.Phases?.Any(p => p.PhaseType == TournamentPhaseType.RoundRobinFinals && 
                                                                     p.FinalsGroup?.Players?.Any() == true) ?? false);
                System.Diagnostics.Debug.WriteLine($"CheckOptionAvailability: HasFinalsPhase = {hasFinalsPhase}");

                if (IncludeFinalsCheckBox != null)
                {
                    IncludeFinalsCheckBox.IsEnabled = hasFinalsPhase;

                    if (!hasFinalsPhase)
                    {
                        IncludeFinalsCheckBox.IsChecked = false;
                    }
                }

                // KO-Phase ist verfügbar wenn in KO-Phase oder bereits abgeschlossen
                var hasKnockoutPhase = (_selectedTournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase) ||
                                     (_selectedTournamentClass.Phases?.Any(p => p.PhaseType == TournamentPhaseType.KnockoutPhase && 
                                                                        (p.WinnerBracket?.Any() == true || p.LoserBracket?.Any() == true)) ?? false);
                System.Diagnostics.Debug.WriteLine($"CheckOptionAvailability: HasKnockoutPhase = {hasKnockoutPhase}");

                if (IncludeKnockoutCheckBox != null)
                {
                    IncludeKnockoutCheckBox.IsEnabled = hasKnockoutPhase;

                    if (!hasKnockoutPhase)
                    {
                        IncludeKnockoutCheckBox.IsChecked = false;
                    }
                }

                if (KnockoutDetailsPanel != null)
                {
                    KnockoutDetailsPanel.IsEnabled = hasKnockoutPhase;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("CheckOptionAvailability: WARNING - KnockoutDetailsPanel is null");
                }

                // Loser Bracket nur verfügbar wenn auch vorhanden
                var hasLoserBracket = hasKnockoutPhase && (_selectedTournamentClass.GetLoserBracketMatches()?.Any() ?? false);
                System.Diagnostics.Debug.WriteLine($"CheckOptionAvailability: HasLoserBracket = {hasLoserBracket}");

                if (IncludeLoserBracketCheckBox != null)
                {
                    IncludeLoserBracketCheckBox.IsEnabled = hasLoserBracket;
                    if (!hasLoserBracket)
                    {
                        IncludeLoserBracketCheckBox.IsChecked = false;
                    }
                }

                // Prüfe ob überhaupt etwas druckbar ist
                var hasPrintableContent = hasGroups || hasFinalsPhase || hasKnockoutPhase;
                System.Diagnostics.Debug.WriteLine($"CheckOptionAvailability: HasPrintableContent = {hasPrintableContent}");

                // Deaktiviere Print-Button wenn nichts druckbar ist
                if (PrintButton != null)
                {
                    PrintButton.IsEnabled = hasPrintableContent;
                }

                System.Diagnostics.Debug.WriteLine("CheckOptionAvailability: Completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckOptionAvailability: ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"CheckOptionAvailability: Stack trace: {ex.StackTrace}");
                
                // Fallback: Deaktiviere alle Controls bei Fehler
                if (IncludeGroupPhaseCheckBox != null)
                    IncludeGroupPhaseCheckBox.IsEnabled = false;
                if (IncludeFinalsCheckBox != null)
                    IncludeFinalsCheckBox.IsEnabled = false;
                if (IncludeKnockoutCheckBox != null)
                    IncludeKnockoutCheckBox.IsEnabled = false;
                if (PrintButton != null)
                    PrintButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Fügt Event-Handler für alle Optionsänderungen hinzu
        /// </summary>
        private void AddOptionChangeHandlers()
        {
            IncludeOverviewCheckBox.Checked += OptionChanged;
            IncludeOverviewCheckBox.Unchecked += OptionChanged;
            
            IncludeFinalsCheckBox.Checked += OptionChanged;
            IncludeFinalsCheckBox.Unchecked += OptionChanged;
            
            IncludeWinnerBracketCheckBox.Checked += OptionChanged;
            IncludeWinnerBracketCheckBox.Unchecked += OptionChanged;
            IncludeLoserBracketCheckBox.Checked += OptionChanged;
            IncludeLoserBracketCheckBox.Unchecked += OptionChanged;
            IncludeKnockoutParticipantsCheckBox.Checked += OptionChanged;
            IncludeKnockoutParticipantsCheckBox.Unchecked += OptionChanged;

            TitleTextBox.TextChanged += OptionChanged;
            SubtitleTextBox.TextChanged += OptionChanged;
        }

        /// <summary>
        /// Event-Handler für Gruppenphase-Checkbox
        /// </summary>
        private void IncludeGroupPhaseCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GroupSelectionPanel != null)
                {
                    GroupSelectionPanel.IsEnabled = true;
                }
                UpdatePreview();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IncludeGroupPhaseCheckBox_Checked: ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Event-Handler für Gruppenphase-Checkbox (deaktiviert)
        /// </summary>
        private void IncludeGroupPhaseCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GroupSelectionPanel != null)
                {
                    GroupSelectionPanel.IsEnabled = false;
                }
                UpdatePreview();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IncludeGroupPhaseCheckBox_Unchecked: ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Event-Handler für "Alle Gruppen"-Checkbox
        /// </summary>
        private void SelectAllGroupsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var checkBox in _groupCheckBoxes)
            {
                checkBox.IsChecked = true;
            }
            UpdatePreview();
        }

        /// <summary>
        /// Event-Handler für "Alle Gruppen"-Checkbox (deaktiviert)
        /// </summary>
        private void SelectAllGroupsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var checkBox in _groupCheckBoxes)
            {
                checkBox.IsChecked = false;
            }
            UpdatePreview();
        }

        /// <summary>
        /// Event-Handler für individuelle Gruppen-Checkboxes
        /// </summary>
        private void GroupCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            // Prüfe ob alle Gruppen ausgewählt sind
            var allSelected = _groupCheckBoxes.All(cb => cb.IsChecked == true);
            var noneSelected = _groupCheckBoxes.All(cb => cb.IsChecked == false);
            
            if (allSelected)
            {
                SelectAllGroupsCheckBox.IsChecked = true;
            }
            else if (noneSelected)
            {
                SelectAllGroupsCheckBox.IsChecked = false;
            }
            else
            {
                SelectAllGroupsCheckBox.IsChecked = null; // Indeterminate state
            }
            
            UpdatePreview();
        }

        /// <summary>
        /// Event-Handler für KO-Phase-Checkbox
        /// </summary>
        private void IncludeKnockoutCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (KnockoutDetailsPanel != null)
                {
                    KnockoutDetailsPanel.IsEnabled = true;
                }
                UpdatePreview();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IncludeKnockoutCheckBox_Checked: ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Event-Handler für KO-Phase-Checkbox (deaktiviert)
        /// </summary>
        private void IncludeKnockoutCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (KnockoutDetailsPanel != null)
                {
                    KnockoutDetailsPanel.IsEnabled = false;
                }
                UpdatePreview();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IncludeKnockoutCheckBox_Unchecked: ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Event-Handler für Vorschau-Button
        /// </summary>
        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== PREVIEW BUTTON CLICKED ===");
                System.Diagnostics.Debug.WriteLine($"Selected Tournament Class: {_selectedTournamentClass.Name}");
                System.Diagnostics.Debug.WriteLine($"Groups in class: {_selectedTournamentClass.Groups.Count}");
                
                foreach (var group in _selectedTournamentClass.Groups)
                {
                    System.Diagnostics.Debug.WriteLine($"  Group: {group.Name} - {group.Players.Count} players, {group.Matches.Count} matches");
                    System.Diagnostics.Debug.WriteLine($"    MatchesGenerated: {group.MatchesGenerated}");
                    
                    var standings = group.GetStandings();
                    System.Diagnostics.Debug.WriteLine($"    GetStandings returned: {standings?.Count ?? -1} standings");
                    
                    if (standings != null && standings.Any())
                    {
                        foreach (var s in standings)
                        {
                            System.Diagnostics.Debug.WriteLine($"      Standing: {s.Player?.Name} - Pos {s.Position}, {s.Points} pts");
                        }
                    }
                }
                
                var options = BuildPrintOptions();
                System.Diagnostics.Debug.WriteLine($"Print Options: Groups={options.IncludeGroupPhase}, Overview={options.IncludeOverview}");
                System.Diagnostics.Debug.WriteLine($"Selected Groups: {string.Join(", ", options.SelectedGroups)}");
                
                // Erstelle Druckvorschau in separatem Fenster
                var preview = _printService.CreatePrintPreview(_selectedTournamentClass, options);
                
                if (preview != null)
                {
                    var previewWindow = new Window
                    {
                        Title = _localizationService?.GetString("PrintPreviewTitle", _selectedTournamentClass.Name) ?? $"Print Preview - {_selectedTournamentClass.Name}",
                        Content = preview,
                        WindowState = WindowState.Maximized,
                        Owner = this
                    };
                    
                    previewWindow.ShowDialog();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("PreviewButton_Click: PrintService returned null preview");
                    var noContentMessage = _localizationService?.GetString("NoContentSelected") ?? "No content selected for display.";
                    var previewTitle = _localizationService?.GetString("PreviewTitle") ?? "Preview";
                    MessageBox.Show(noContentMessage, previewTitle, 
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PreviewButton_Click: ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"PreviewButton_Click: Stack trace: {ex.StackTrace}");
                var errorMessage = _localizationService?.GetString("PreviewError", ex.Message) ?? $"Error during preview: {ex.Message}";
                var errorTitle = _localizationService?.GetString("Error") ?? "Error";
                MessageBox.Show(errorMessage, errorTitle, 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Event-Handler für Drucken-Button
        /// </summary>
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var options = BuildPrintOptions();
                
                // Validierung
                if (!ValidatePrintOptions(options))
                {
                    return;
                }
                
                PrintOptions = options;
                PrintConfirmed = true;
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                var errorMessage = _localizationService?.GetString("PrintPreparationError", ex.Message) ?? $"Error during print preparation: {ex.Message}";
                var errorTitle = _localizationService?.GetString("Error") ?? "Error";
                MessageBox.Show(errorMessage, errorTitle, 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Event-Handler für Abbrechen-Button
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            PrintConfirmed = false;
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Generischer Event-Handler für Optionsänderungen
        /// </summary>
        private void OptionChanged(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }

        /// <summary>
        /// Aktualisiert die Druckvorschau basierend auf den aktuellen Optionen
        /// </summary>
        private void UpdatePreview()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("UpdatePreview: Starting preview update");
                
                if (PreviewPanel == null)
                {
                    System.Diagnostics.Debug.WriteLine("UpdatePreview: PreviewPanel is null - skipping update");
                    return;
                }

                var options = BuildPrintOptions();
                var previewInfo = GeneratePreviewInfo(options);
                
                PreviewPanel.Children.Clear();
                
                foreach (var info in previewInfo)
                {
                    var textBlock = new TextBlock
                    {
                        Text = info,
                        Margin = new Thickness(5),
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap
                    };
                    
                    PreviewPanel.Children.Add(textBlock);
                }
                
                if (previewInfo.Count == 0)
                {
                    var noContentText = _localizationService?.GetString("NoContentToPrint") ?? "📋 No content selected for printing";
                    var noContentBlock = new TextBlock
                    {
                        Text = noContentText,
                        Foreground = System.Windows.Media.Brushes.Orange,
                        FontStyle = FontStyles.Italic,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(20)
                    };
                    PreviewPanel.Children.Add(noContentBlock);
                }
                
                System.Diagnostics.Debug.WriteLine($"UpdatePreview: Successfully updated preview with {previewInfo.Count} items");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePreview: ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"UpdatePreview: Stack trace: {ex.StackTrace}");
                
                if (PreviewPanel != null)
                {
                    PreviewPanel.Children.Clear();
                    var errorText = _localizationService?.GetString("PreviewError2", ex.Message) ?? $"❌ Error during preview: {ex.Message}";
                    var errorBlock = new TextBlock
                    {
                        Text = errorText,
                        Foreground = System.Windows.Media.Brushes.Red,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(10)
                    };
                    PreviewPanel.Children.Add(errorBlock);
                }
            }
        }

        /// <summary>
        /// Erstellt die Druckoptionen basierend auf den UI-Einstellungen
        /// </summary>
        private TournamentPrintOptions BuildPrintOptions()
        {
            try
            {
                var selectedGroupIds = _groupCheckBoxes
                    .Where(cb => cb?.IsChecked == true && cb.Tag != null)
                    .Select(cb => (int)cb.Tag)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"BuildPrintOptions: Selected {selectedGroupIds.Count} groups");

                return new TournamentPrintOptions
                {
                    ShowPrintDialog = true,
                    IncludeOverview = IncludeOverviewCheckBox?.IsChecked == true,
                    IncludeGroupPhase = IncludeGroupPhaseCheckBox?.IsChecked == true,
                    SelectedGroups = selectedGroupIds,
                    IncludeFinalsPhase = IncludeFinalsCheckBox?.IsChecked == true,
                    IncludeKnockoutPhase = IncludeKnockoutCheckBox?.IsChecked == true,
                    IncludeWinnerBracket = IncludeWinnerBracketCheckBox?.IsChecked == true,
                    IncludeLoserBracket = IncludeLoserBracketCheckBox?.IsChecked == true,
                    IncludeKnockoutParticipants = IncludeKnockoutParticipantsCheckBox?.IsChecked == true,
                    Title = TitleTextBox?.Text?.Trim() ?? "",
                    Subtitle = SubtitleTextBox?.Text?.Trim() ?? ""
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BuildPrintOptions: ERROR: {ex.Message}");
                
                // Fallback: Nur Übersicht drucken
                return new TournamentPrintOptions
                {
                    ShowPrintDialog = true,
                    IncludeOverview = true,
                    IncludeGroupPhase = false,
                    SelectedGroups = new List<int>(),
                    IncludeFinalsPhase = false,
                    IncludeKnockoutPhase = false,
                    IncludeWinnerBracket = false,
                    IncludeLoserBracket = false,
                    IncludeKnockoutParticipants = false,
                    Title = "",
                    Subtitle = ""
                };
            }
        }

        /// <summary>
        /// Generiert Vorschau-Informationen für die ausgewählten Optionen
        /// </summary>
        private List<string> GeneratePreviewInfo(TournamentPrintOptions options)
        {
            try
            {
                var info = new List<string>();
                var pageCount = 0;

                if (options.IncludeOverview)
                {
                    info.Add(_localizationService?.GetString("PageOverview", pageCount + 1) ?? $"📄 Page {pageCount + 1}: Tournament Overview");
                    info.Add(_localizationService?.GetString("OverviewContent1") ?? "   • General tournament information");
                    info.Add(_localizationService?.GetString("OverviewContent2") ?? "   • Game rules and phase status");
                    info.Add(_localizationService?.GetString("OverviewContent3") ?? "   • Groups overview");
                    pageCount++;
                }

                if (options.IncludeGroupPhase && (_selectedTournamentClass.Groups?.Any() ?? false))
                {
                    var selectedGroups = options.SelectedGroups.Any() 
                        ? _selectedTournamentClass.Groups.Where(g => options.SelectedGroups.Contains(g.Id))
                        : _selectedTournamentClass.Groups;

                    foreach (var group in selectedGroups)
                    {
                        if (group == null) continue;
                        
                        pageCount++;
                        info.Add(_localizationService?.GetString("PageGroupPhase", pageCount, group.Name) ?? $"📄 Page {pageCount}: Group Phase - {group.Name}");
                        info.Add(_localizationService?.GetString("GroupPlayers", group.Players?.Count ?? 0) ?? $"   • {group.Players?.Count ?? 0} Players");
                        info.Add(_localizationService?.GetString("GroupMatches", group.Matches?.Count ?? 0) ?? $"   • {group.Matches?.Count ?? 0} Matches");
                        info.Add(_localizationService?.GetString("GroupContent") ?? "   • Standings and results");
                    }
                }

                if (options.IncludeFinalsPhase && _selectedTournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
                {
                    pageCount++;
                    info.Add(_localizationService?.GetString("PageFinals", pageCount) ?? $"📄 Page {pageCount}: Finals");
                    info.Add(_localizationService?.GetString("FinalsContent1") ?? "   • Qualified finalists");
                    info.Add(_localizationService?.GetString("FinalsContent2") ?? "   • Finals standings");
                    info.Add(_localizationService?.GetString("FinalsContent3") ?? "   • Finals matches");
                }

                if (options.IncludeKnockoutPhase && _selectedTournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
                {
                    if (options.IncludeWinnerBracket)
                    {
                        pageCount++;
                        var winnerMatches = _selectedTournamentClass.GetWinnerBracketMatches();
                        info.Add(_localizationService?.GetString("PageWinnerBracket", pageCount) ?? $"📄 Page {pageCount}: Winner Bracket");
                        info.Add(_localizationService?.GetString("WinnerBracketMatches", winnerMatches.Count) ?? $"   • {winnerMatches.Count} KO matches");
                    }

                    if (options.IncludeLoserBracket && (_selectedTournamentClass.GetLoserBracketMatches()?.Any() ?? false))
                    {
                        pageCount++;
                        var loserMatches = _selectedTournamentClass.GetLoserBracketMatches();
                        info.Add(_localizationService?.GetString("PageLoserBracket", pageCount) ?? $"📄 Page {pageCount}: Loser Bracket");
                        info.Add(_localizationService?.GetString("LoserBracketMatches", loserMatches.Count) ?? $"   • {loserMatches.Count} LB matches");
                    }

                    if (options.IncludeKnockoutParticipants)
                    {
                        pageCount++;
                        var participants = _selectedTournamentClass.CurrentPhase?.QualifiedPlayers?.Count ?? 0;
                        info.Add(_localizationService?.GetString("PageKnockoutParticipants", pageCount) ?? $"📄 Page {pageCount}: KO Participants");
                        info.Add(_localizationService?.GetString("KnockoutParticipantsContent", participants) ?? $"   • {participants} qualified players");
                    }
                }

                return info;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GeneratePreviewInfo: ERROR: {ex.Message}");
                var errorText = _localizationService?.GetString("PreviewGenerationError", ex.Message) ?? $"❌ Error generating preview information: {ex.Message}";
                return new List<string> { errorText };
            }
        }

        /// <summary>
        /// Validiert die Druckoptionen
        /// </summary>
        private bool ValidatePrintOptions(TournamentPrintOptions options)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ValidatePrintOptions: Starting validation");
                
                // Prüfe ob mindestens eine Option ausgewählt ist
                if (!options.IncludeOverview && !options.IncludeGroupPhase && 
                    !options.IncludeFinalsPhase && !options.IncludeKnockoutPhase)
                {
                    var message = _localizationService?.GetString("SelectAtLeastOne") ?? "Please select at least one print option.";
                    var title = _localizationService?.GetString("NoSelection") ?? "No Selection";
                    MessageBox.Show(message, title, 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Prüfe ob bei Gruppenphase mindestens eine Gruppe ausgewählt ist
                if (options.IncludeGroupPhase)
                {
                    var hasGroups = _selectedTournamentClass.Groups?.Any() ?? false;
                    if (!hasGroups)
                    {
                        System.Diagnostics.Debug.WriteLine("ValidatePrintOptions: Group phase selected but no groups available");
                        var message = _localizationService?.GetString("NoGroupsAvailable") ?? "The selected tournament class contains no groups to print.";
                        var title = _localizationService?.GetString("NoGroupsAvailableTitle") ?? "No Groups Available";
                        MessageBox.Show(message, title, 
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    
                    if (!options.SelectedGroups.Any())
                    {
                        var message = _localizationService?.GetString("SelectAtLeastOneGroup") ?? "Please select at least one group.";
                        var title = _localizationService?.GetString("NoGroupSelected") ?? "No Group Selected";
                        MessageBox.Show(message, title, 
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    
                    // Prüfe ob die ausgewählten Gruppen tatsächlich existieren
                    var existingGroupIds = _selectedTournamentClass.Groups.Select(g => g.Id).ToList();
                    var validSelectedGroups = options.SelectedGroups.Where(id => existingGroupIds.Contains(id)).ToList();
                    
                    if (!validSelectedGroups.Any())
                    {
                        var message = _localizationService?.GetString("InvalidGroupSelection") ?? "The selected groups are no longer available.";
                        var title = _localizationService?.GetString("InvalidGroupSelectionTitle") ?? "Invalid Group Selection";
                        MessageBox.Show(message, title, 
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    
                    // Aktualisiere die Options mit den gültigen Gruppen
                    options.SelectedGroups = validSelectedGroups;
                }

                // Prüfe Finals-Phase
                if (options.IncludeFinalsPhase)
                {
                    var hasFinalsPhase = (_selectedTournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals) ||
                                       (_selectedTournamentClass.Phases?.Any(p => p.PhaseType == TournamentPhaseType.RoundRobinFinals) ?? false);
                    
                    if (!hasFinalsPhase)
                    {
                        System.Diagnostics.Debug.WriteLine("ValidatePrintOptions: Finals phase selected but not available");
                        var message = _localizationService?.GetString("NoFinalsAvailable") ?? "The selected tournament class has no finals to print.";
                        var title = _localizationService?.GetString("NoFinalsAvailableTitle") ?? "No Finals Available";
                        MessageBox.Show(message, title, 
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }

                // Prüfe bei KO-Phase mindestens ein Bracket ausgewählt ist
                if (options.IncludeKnockoutPhase)
                {
                    var hasKnockoutPhase = (_selectedTournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase) ||
                                         (_selectedTournamentClass.Phases?.Any(p => p.PhaseType == TournamentPhaseType.KnockoutPhase) ?? false);
                    
                    if (!hasKnockoutPhase)
                    {
                        System.Diagnostics.Debug.WriteLine("ValidatePrintOptions: Knockout phase selected but not available");
                        var message = _localizationService?.GetString("NoKnockoutAvailable") ?? "The selected tournament class has no knockout phase to print.";
                        var title = _localizationService?.GetString("NoKnockoutAvailableTitle") ?? "No Knockout Phase Available";
                        MessageBox.Show(message, title, 
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    
                    if (!options.IncludeWinnerBracket && !options.IncludeLoserBracket && !options.IncludeKnockoutParticipants)
                    {
                        var message = _localizationService?.GetString("SelectAtLeastOneKO") ?? "Please select at least one KO option.";
                        var title = _localizationService?.GetString("NoKOOptionSelected") ?? "No KO Option Selected";
                        MessageBox.Show(message, title, 
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }

                System.Diagnostics.Debug.WriteLine("ValidatePrintOptions: Validation successful");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ValidatePrintOptions: ERROR: {ex.Message}");
                var errorMessage = _localizationService?.GetString("ValidationError", ex.Message) ?? $"Validation error: {ex.Message}";
                var errorTitle = _localizationService?.GetString("ValidationErrorTitle") ?? "Validation Error";
                MessageBox.Show(errorMessage, errorTitle, 
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}