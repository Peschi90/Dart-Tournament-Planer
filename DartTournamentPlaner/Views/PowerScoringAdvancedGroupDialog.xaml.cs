using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media;
using DartTournamentPlaner.Models.PowerScore;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.PowerScore;

namespace DartTournamentPlaner.Views;

public class ClassSelectionItem : INotifyPropertyChanged
{
    private string _name = "";
    private bool _isSelected;

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class GroupDisplayItem
{
    public string GroupName { get; set; } = "";
    public List<PowerScoringPlayer> Players { get; set; } = new();
}

/// <summary>
/// Erweiterter Dialog für Gruppeneinteilung mit Klassen, Gruppen und Spieler-Limits
/// </summary>
public partial class PowerScoringAdvancedGroupDialog : Window
{
    private readonly PowerScoringService _powerScoringService;
    private readonly LocalizationService _localizationService;
    private readonly ObservableCollection<ClassSelectionItem> _classItems = new();
    private List<GroupDistributionResult> _currentDistribution = new();
    
    // ✅ Speichere erweiterte Config zwischen Dialog-Aufrufen
    private GroupDistributionConfig? _advancedConfig = null;
    
    // ✅ FIX: Flag um parallele Generierung zu verhindern
    private bool _isGenerating = false;
    
    // ✅ NEU: Service für Tournament-Konvertierung
    private readonly PowerScoringToTournamentService _tournamentConversionService;
    
    // ✅ PHASE 3: Service für Tournament-Management
    private readonly TournamentManagementService? _tournamentManagementService;
    private readonly Window? _parentWindow;
    private readonly MainWindow? _mainWindow; // ✅ PHASE 3: MainWindow für UI-Refresh

    public PowerScoringAdvancedGroupDialog(
        PowerScoringService powerScoringService,
        LocalizationService localizationService,
        TournamentManagementService? tournamentManagementService = null,
        Window? parentWindow = null,
        MainWindow? mainWindow = null) // ✅ PHASE 3
    {
        InitializeComponent();
        
        _powerScoringService = powerScoringService;
        _localizationService = localizationService;
        _tournamentConversionService = new PowerScoringToTournamentService();
        _tournamentManagementService = tournamentManagementService; // ✅ PHASE 3
        _parentWindow = parentWindow; // ✅ PHASE 3
        _mainWindow = mainWindow; // ✅ PHASE 3

        InitializeClassSelection();
        
        // ✅ FIX: Warte bis Window geladen ist bevor UI aktualisiert wird
        Loaded += (s, e) =>
        {
            UpdateTranslations();
            UpdateHeaderTranslations(); // ✅ NEU: Übersetze Header nach Laden
            
            // ✅ FIX: Generiere initiale Distribution
            GenerateDistribution();
        };
    }
    
    /// <summary>
    /// ✅ NEU: Aktualisiert alle UI-Übersetzungen
    /// </summary>
    private void UpdateTranslations()
    {
        try
        {
            // Window Title
            Title = _localizationService.GetString("PowerScoring_GroupDistribution_Title");
            
            // Labels (exist bereits)
            SelectClassesLabel.Text = _localizationService.GetString("PowerScoring_GroupDistribution_SelectClasses");
            GroupsPerClassLabel.Text = _localizationService.GetString("PowerScoring_GroupDistribution_GroupsPerClass");
            PlayersPerGroupLabel.Text = _localizationService.GetString("PowerScoring_GroupDistribution_PlayersPerGroup");
            
            // Buttons
            GenerateButton.Content = _localizationService.GetString("PowerScoring_GroupDistribution_Generate");
            CopyButton.Content = _localizationService.GetString("PowerScoring_GroupDistribution_Export");
            CloseButton.Content = _localizationService.GetString("PowerScoring_GroupDistribution_Cancel");
            AdvancedSettingsButton.Content = _localizationService.GetString("PowerScoring_GroupDistribution_Advanced");
            CreateTournamentButton.Content = _localizationService.GetString("PowerScoring_CreateTournament_Create"); // ✅ NEU
            
            // ComboBox Items übersetzen
            UpdateComboBoxTranslations();
            
            System.Diagnostics.Debug.WriteLine("✅ PowerScoringAdvancedGroupDialog translations updated");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Error updating translations: {ex.Message}");
        }
    }
    
    /// <summary>
    /// ✅ NEU: Wird aufgerufen wenn Window geladen ist, um Header zu übersetzen
    /// </summary>
    private void UpdateHeaderTranslations()
    {
        try
        {
            // Finde Header-Elemente im Visual Tree (da sie jetzt x:Name haben)
            var headerTitle = FindName("HeaderTitleText") as TextBlock;
            if (headerTitle != null)
            {
                headerTitle.Text = _localizationService.GetString("PowerScoring_GroupDistribution_Title");
            }
            
            var headerDesc = FindName("HeaderDescriptionText") as TextBlock;
            if (headerDesc != null)
            {
                headerDesc.Text = _localizationService.GetString("PowerScoring_GroupDistribution_Description");
            }
            
            var previewLabel = FindName("DistributionPreviewLabel") as TextBlock;
            if (previewLabel != null)
            {
                previewLabel.Text = _localizationService.GetString("PowerScoring_GroupDistribution_DistributionPreview");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Error updating header translations: {ex.Message}");
        }
    }
    
    /// <summary>
    /// ✅ NEU: Übersetzt ComboBox Items
    /// </summary>
    private void UpdateComboBoxTranslations()
    {
        // Groups per Class ComboBox
        var groupsItems = GroupsPerClassComboBox.Items.OfType<ComboBoxItem>().ToList();
        if (groupsItems.Count >= 4)
        {
            groupsItems[0].Content = _localizationService.GetString("PowerScoring_GroupDistribution_1Group");
            groupsItems[1].Content = _localizationService.GetString("PowerScoring_GroupDistribution_2Groups");
            groupsItems[2].Content = _localizationService.GetString("PowerScoring_GroupDistribution_3Groups");
            groupsItems[3].Content = _localizationService.GetString("PowerScoring_GroupDistribution_4Groups");
        }
        
        // Players per Group ComboBox
        var playersItems = PlayersPerGroupComboBox.Items.OfType<ComboBoxItem>().ToList();
        if (playersItems.Count >= 5)
        {
            playersItems[0].Content = _localizationService.GetString("PowerScoring_GroupDistribution_2Players");
            playersItems[1].Content = _localizationService.GetString("PowerScoring_GroupDistribution_3Players");
            playersItems[2].Content = _localizationService.GetString("PowerScoring_GroupDistribution_4Players");
            playersItems[3].Content = _localizationService.GetString("PowerScoring_GroupDistribution_5Players");
            playersItems[4].Content = _localizationService.GetString("PowerScoring_GroupDistribution_6Players");
        }
    }

    /// <summary>
    /// ✅ NEU: Aktualisiert Button-Übersetzungen
    /// </summary>
    private void UpdateButtonTranslations()
    {
        // Finde alle Buttons im Action Panel
        var buttons = FindVisualChildren<Button>(this).Where(b => 
            b.Content?.ToString()?.Contains("Generate") == true ||
            b.Content?.ToString()?.Contains("Copy") == true ||
            b.Content?.ToString()?.Contains("Close") == true ||
            b.Content?.ToString()?.Contains("Advanced") == true
        ).ToList();
        
        foreach (var button in buttons)
        {
            var content = button.Content?.ToString() ?? "";
            
            if (content.Contains("Generate"))
                button.Content = _localizationService.GetString("PowerScoring_GroupDistribution_Generate");
            else if (content.Contains("Copy"))
                button.Content = _localizationService.GetString("PowerScoring_GroupDistribution_Export");
            else if (content.Contains("Close"))
                button.Content = _localizationService.GetString("PowerScoring_GroupDistribution_Cancel");
            else if (content.Contains("Advanced"))
                button.Content = _localizationService.GetString("PowerScoring_GroupDistribution_Advanced");
        }
    }
    
    /// <summary>
    /// ✅ NEU: Findet alle Children eines bestimmten Typs
    /// </summary>
    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj == null) yield break;
        
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);
            
            if (child is T typedChild)
            {
                yield return typedChild;
            }
            
            foreach (var childOfChild in FindVisualChildren<T>(child))
            {
                yield return childOfChild;
            }
        }
    }

    private void InitializeClassSelection()
    {
        // Standard-Auswahl: Erste 4 Klassen
        foreach (var className in GroupDistributionConfig.AvailableClasses)
        {
            _classItems.Add(new ClassSelectionItem
            {
                Name = GetClassDisplayName(className),
                IsSelected = _classItems.Count < 4 // Standard: Platin, Gold, Silber, Bronze
            });
        }
        
        ClassCheckBoxes.ItemsSource = _classItems;
        
        // ✅ FIX: Subscribe zu PropertyChanged Events für auto-regenerate
        foreach (var item in _classItems)
        {
            item.PropertyChanged += (s, e) =>
            {
                // ✅ FIX: Nur reagieren wenn IsSelected geändert wurde UND Window geladen ist UND nicht am Generieren
                if (e.PropertyName == nameof(ClassSelectionItem.IsSelected) && IsLoaded && !_isGenerating)
                {
                    System.Diagnostics.Debug.WriteLine($"🔄 Class selection changed: {item.Name} = {item.IsSelected}");
                    GenerateDistribution();
                }
            };
        }
    }

    private string GetClassDisplayName(string className)
    {
        return className switch
        {
            "Platin" => "🏆 Platin",
            "Gold" => "🥇 Gold",
            "Silber" => "🥈 Silber",
            "Bronze" => "🥉 Bronze",
            "Eisen" => "⚙️ Eisen",
            _ => className
        };
    }

    private void ConfigChanged(object sender, SelectionChangedEventArgs e)
    {
        // ✅ FIX: Nur ausführen wenn Window vollständig geladen UND nicht bereits am Generieren
        if (!IsLoaded || _isGenerating) return;
        
        // ✅ FIX: Debounce - Verhindere mehrfaches schnelles Regenerieren
        System.Diagnostics.Debug.WriteLine("🔄 Config changed, regenerating distribution...");
        GenerateDistribution();
    }
    
    /// <summary>
    /// ✅ Event-Handler für Generate Distribution Button
    /// </summary>
    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("🎲 Generate button clicked");
        GenerateDistribution();
    }
    
    /// <summary>
    /// ✅ NEU: Öffnet erweiterte Einstellungen
    /// </summary>
    private void AdvancedSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var currentConfig = GetCurrentConfig();
            
            // ✅ FIX: Dialog gibt modifizierte Config zurück
            if (PowerScoringAdvancedConfigDialog.ShowDialog(currentConfig, _localizationService, this))
            {
                System.Diagnostics.Debug.WriteLine($"📊 Advanced settings applied: Mode={currentConfig.Mode}");
                System.Diagnostics.Debug.WriteLine($"   Min/Max Players: {currentConfig.MinPlayersPerGroup}-{currentConfig.MaxPlayersPerGroup}");
                System.Diagnostics.Debug.WriteLine($"   Class Rules: {currentConfig.ClassRules.Count} rules");
                
                // ✅ NEU: Wende die Config-Änderungen an (speichere die modifizierte Config)
                ApplyAdvancedConfig(currentConfig);
                
                // ✅ FIX: Regeneriere Distribution AUTOMATISCH mit neuen Einstellungen
                GenerateDistribution();
                
                // ✅ Success-Meldung zeigen
                PowerScoringConfirmDialog.ShowSuccess(
                    _localizationService.GetString("PowerScoring_AdvancedSettings_SettingsApplied"),
                    _localizationService.GetString("PowerScoring_AdvancedSettings_SettingsAppliedMessage") +
                    $"\n\n{_localizationService.GetString("PowerScoring_AdvancedSettings_DistributionMode")}: {currentConfig.Mode}\n" +
                    $"{_localizationService.GetString("PowerScoring_AdvancedSettings_PlayerLimits")}: {currentConfig.MinPlayersPerGroup}-{currentConfig.MaxPlayersPerGroup}\n\n" +
                    _localizationService.GetString("PowerScoring_GroupDistribution_Generate") + " ✅",
                    this);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error opening advanced settings: {ex.Message}");
            PowerScoringConfirmDialog.ShowError(
                _localizationService.GetString("Error"),
                $"{_localizationService.GetString("Error")}: {ex.Message}",
                this);
        }
    }
    
    /// <summary>
    /// ✅ NEU: Wendet die erweiterten Config-Einstellungen an
    /// </summary>
    private void ApplyAdvancedConfig(GroupDistributionConfig config)
    {
        // ✅ FIX: Speichere Config in Member-Variable
        _advancedConfig = config;
        
        System.Diagnostics.Debug.WriteLine($"✅ Applied advanced config:");
        System.Diagnostics.Debug.WriteLine($"   Mode: {config.Mode}");
        System.Diagnostics.Debug.WriteLine($"   Min/Max Players: {config.MinPlayersPerGroup}-{config.MaxPlayersPerGroup}");
        System.Diagnostics.Debug.WriteLine($"   Class Rules: {config.ClassRules.Count}");
        
        foreach (var rule in config.ClassRules)
        {
            System.Diagnostics.Debug.WriteLine($"   - {rule.Key}: Groups={rule.Value.CustomGroupCount}, Skip={rule.Value.Skip}");
        }
    }

    private void GenerateDistribution()
    {
        // ✅ FIX: Verhindere parallele Ausführung
        if (_isGenerating)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Already generating, skipping...");
            return;
        }
        
        _isGenerating = true;
        
        try
        {
            var config = GetCurrentConfig();
            
            System.Diagnostics.Debug.WriteLine($"📊 Starting distribution generation:");
            System.Diagnostics.Debug.WriteLine($"   Selected Classes: {config.SelectedClasses.Count}");
            System.Diagnostics.Debug.WriteLine($"   Groups per Class: {config.GroupsPerClass}");
            System.Diagnostics.Debug.WriteLine($"   Players per Group: {config.PlayersPerGroup}");
            System.Diagnostics.Debug.WriteLine($"   Mode: {config.Mode}");
            
            if (config.SelectedClasses.Count == 0)
            {
                PowerScoringConfirmDialog.ShowWarning(
                    _localizationService.GetString("PowerScoring_Warning_NoClasses"),
                    _localizationService.GetString("PowerScoring_Warning_SelectClasses"),
                    IsLoaded ? this : null);
                return;
            }

            var rankedPlayers = _powerScoringService.GetRankedPlayers();
            
            System.Diagnostics.Debug.WriteLine($"   Ranked Players: {rankedPlayers.Count}");
            
            if (rankedPlayers.Count == 0)
            {
                PowerScoringConfirmDialog.ShowWarning(
                    _localizationService.GetString("PowerScoring_Error_NoPlayers"),
                    "No players found for distribution.",
                    IsLoaded ? this : null);
                return;
            }

            // Verteile Spieler auf Klassen und Gruppen
            _currentDistribution = DistributePlayersAdvanced(rankedPlayers, config);
            
            System.Diagnostics.Debug.WriteLine($"✅ Distribution calculated: {_currentDistribution.Count} groups");
            
            // ✅ FIX: Zeige Ergebnis in UI
            DisplayDistribution();
            
            System.Diagnostics.Debug.WriteLine($"📊 Distribution generation complete");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error generating distribution: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
            PowerScoringConfirmDialog.ShowError(
                _localizationService.GetString("Error"),
                $"Error generating distribution: {ex.Message}",
                IsLoaded ? this : null);
        }
        finally
        {
            // ✅ FIX: Immer Flag zurücksetzen
            _isGenerating = false;
        }
    }
    
    /// <summary>
    /// ✅ NEU: Zeigt die Distribution in der UI an
    /// </summary>
    private void DisplayDistribution()
    {
        try
        {
            // ✅ FIX: Dispatcher verwenden um Thread-Safety zu garantieren
            Dispatcher.Invoke(() =>
            {
                // Erstelle Display-Items für die UI
                var displayItems = _currentDistribution.Select(group => new GroupDisplayItem
                {
                    GroupName = group.GetGroupDisplayName(),
                    Players = group.Players.ToList()
                }).ToList();
                
                // ✅ FIX: Komplett neues ItemsSource setzen (kein null-Trick mehr)
                GroupsDisplay.ItemsSource = displayItems;
                
                // ✅ FIX: Force UI Update
                GroupsDisplay.Items.Refresh();
                
                // ✅ NEU: Zeige "Create Tournament" Button wenn Distribution vorhanden
                CreateTournamentButton.Visibility = _currentDistribution.Count > 0 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
                
                System.Diagnostics.Debug.WriteLine($"✅ Display updated with {displayItems.Count} groups:");
                
                foreach (var item in displayItems)
                {
                    System.Diagnostics.Debug.WriteLine($"   - {item.GroupName}: {item.Players.Count} players");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error displaying distribution: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
        }
    }
    
    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentDistribution.Count == 0)
        {
            PowerScoringConfirmDialog.ShowWarning(
                "No Distribution",
                "Please generate a distribution first.",
                this);
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("=== PowerScoring Advanced Group Distribution ===");
        sb.AppendLine();

        foreach (var group in _currentDistribution)
        {
            sb.AppendLine($"{group.GetGroupDisplayName()}:");
            sb.AppendLine(new string('-', 50));
            
            foreach (var player in group.Players)
            {
                sb.AppendLine($"  • {player.Name} - Avg: {player.AverageScore:F2} (Total: {player.TotalScore})");
            }
            
            sb.AppendLine();
        }

        try
        {
            Clipboard.SetText(sb.ToString());
            PowerScoringConfirmDialog.ShowSuccess(
                "Success",
                "Distribution copied to clipboard!",
                this);
        }
        catch (Exception ex)
        {
            PowerScoringConfirmDialog.ShowError(
                "Error",
                $"Error copying to clipboard: {ex.Message}",
                this);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
    
    /// <summary>
    /// ✅ NEU: Handler für "Create Tournament" Button
    /// </summary>
    private async void CreateTournamentButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🏆 Create Tournament button clicked");
            
            // 1. Validiere Distribution
            var validation = _tournamentConversionService.ValidateDistribution(_currentDistribution);
            
            if (!validation.IsValid)
            {
                PowerScoringConfirmDialog.ShowError(
                    _localizationService.GetString("Error"),
                    validation.GetSummary(),
                    this);
                return;
            }
            
            // 2. Erstelle Preview
            var preview = _tournamentConversionService.CreatePreview(_currentDistribution);
            
            // 3. Prüfe ob bestehendes Turnier vorhanden
            bool hasPendingTournament = _tournamentManagementService?.HasActiveTournament() ?? false;
            
            System.Diagnostics.Debug.WriteLine($"   Has pending tournament: {hasPendingTournament}");
            
            // 4. Zeige Confirmation Dialog
            var confirmed = PowerScoringCreateTournamentDialog.ShowDialog(
                preview,
                hasPendingTournament,
                _localizationService,
                this);
            
            if (confirmed == true)
            {
                // 5. Erstelle Turnier (Phase 3) - async call
                await CreateTournamentFromDistribution();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error in CreateTournamentButton_Click: {ex.Message}");
            PowerScoringConfirmDialog.ShowError(
                _localizationService.GetString("Error"),
                $"Error creating tournament: {ex.Message}",
                this);
        }
    }
    
    /// <summary>
    /// ✅ NEU: Erstellt Turnier aus Distribution (Phase 3)
    /// </summary>
    private async Task CreateTournamentFromDistribution()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🏗️ Creating tournament from distribution...");
            
            // 1. Validiere dass Services vorhanden sind
            if (_tournamentManagementService == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ TournamentManagementService not available");
                PowerScoringConfirmDialog.ShowError(
                    _localizationService.GetString("Error"),
                    "Tournament Management Service is not available. Please restart the application.",
                    this);
                return;
            }
            
            // 2. Konvertiere Distribution zu TournamentClasses
            System.Diagnostics.Debug.WriteLine("🔄 Converting distribution to tournament classes...");
            var tournamentClasses = _tournamentConversionService.ConvertDistributionToTournamentClasses(_currentDistribution);
            
            System.Diagnostics.Debug.WriteLine($"✅ Converted to {tournamentClasses.Count} tournament classes");
            
            // 3. Erstelle Turnier über TournamentManagementService
            System.Diagnostics.Debug.WriteLine("🏗️ Creating tournament...");
            if (!await _tournamentManagementService.CreateTournamentFromPowerScoringAsync(tournamentClasses))
            {
                PowerScoringConfirmDialog.ShowError(
                    _localizationService.GetString("Error"),
                    "Failed to create tournament. Please check the logs.",
                    this);
                return;
            }
            
            System.Diagnostics.Debug.WriteLine("✅ Tournament created successfully!");
            
            // 4. Zeige Success-Message
            PowerScoringConfirmDialog.ShowSuccess(
                _localizationService.GetString("PowerScoring_CreateTournament_Success_Title"),
                _localizationService.GetString("PowerScoring_CreateTournament_Success_Message"),
                this);
            
            // 5. ✅ WICHTIG: Aktualisiere MainWindow UI VOR dem Schließen
            if (_mainWindow != null)
            {
                System.Diagnostics.Debug.WriteLine("🔄 Refreshing MainWindow UI with new tournament data...");
                _mainWindow.RefreshTournamentData();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ MainWindow reference is null - UI will not refresh automatically");
            }
            
            // 6. Schließe alle PowerScoring-Dialoge
            System.Diagnostics.Debug.WriteLine("🔄 Closing PowerScoring dialogs...");
            
            // Schließe diesen Dialog
            DialogResult = true;
            Close();
            
            // Finde und schließe PowerScoringWindow (Parent)
            if (_parentWindow != null)
            {
                System.Diagnostics.Debug.WriteLine("   Closing PowerScoringWindow...");
                _parentWindow.Close();
            }
            
            // MainWindow wird automatisch von PowerScoringWindow.Closed Event geöffnet
            System.Diagnostics.Debug.WriteLine("✅ Tournament creation flow complete!");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error creating tournament: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
            
            PowerScoringConfirmDialog.ShowError(
                _localizationService.GetString("Error"),
                $"Error creating tournament: {ex.Message}",
                this);
        }
    }
    
    // =====================================
    // CONFIGURATION & DISTRIBUTION METHODS
    // =====================================
    
    private GroupDistributionConfig GetCurrentConfig()
    {
        // ✅ FIX: Wenn Advanced Config existiert, verwende diese als Basis
        var config = _advancedConfig ?? new GroupDistributionConfig();
        
        // Hole gewählte Klassen (überschreibt Advanced Config)
        config.SelectedClasses.Clear();
        for (int i = 0; i < _classItems.Count; i++) // ✅ FIX: "int" statt "numér"
        {
            if (_classItems[i].IsSelected)
            {
                config.SelectedClasses.Add(GroupDistributionConfig.AvailableClasses[i]);
            }
        }
        
        // Gruppen pro Klasse - mit Null-Check (überschreibt nur wenn nicht durch Advanced Config gesetzt)
        if (GroupsPerClassComboBox != null && GroupsPerClassComboBox.SelectedIndex >= 0)
        {
            // Nur setzen wenn keine klassen-spezifischen Regeln existieren
            if (config.ClassRules.Count == 0)
            {
                config.GroupsPerClass = GroupsPerClassComboBox.SelectedIndex + 1;
            }
        }
        else
        {
            if (config.ClassRules.Count == 0)
            {
                config.GroupsPerClass = 1; // Default
            }
        }
        
        // ✅ FIX: Spieler pro Gruppe -> Nur wenn nicht durch Advanced Config gesetzt
        if (_advancedConfig == null || _advancedConfig.MaxPlayersPerGroup == 6) // Default
        {
            if (PlayersPerGroupComboBox != null && PlayersPerGroupComboBox.SelectedIndex >= 0)
            {
                config.PlayersPerGroup = PlayersPerGroupComboBox.SelectedIndex + 2;
                config.MaxPlayersPerGroup = config.PlayersPerGroup;
            }
            else
            {
                config.PlayersPerGroup = 4; // Default
                config.MaxPlayersPerGroup = 4;
            }
        }
        
        return config;
    }

    private List<GroupDistributionResult> DistributePlayersAdvanced(
        List<PowerScoringPlayer> rankedPlayers,
        GroupDistributionConfig config)
    {
        var results = new List<GroupDistributionResult>();
        
        // ✅ NEU: Sortiere oder randomisiere Spieler basierend auf Mode
        var distributionPlayers = PreparePlayersForDistribution(rankedPlayers, config.Mode);
        
        var totalGroups = config.SelectedClasses.Sum(className => config.GetGroupsForClass(className));
        
        // ✅ FIX: Berechne Spieler pro Gruppe unter Berücksichtigung von Min/Max
        var playersPerGroup = Math.Min(
            config.MaxPlayersPerGroup, 
            Math.Max(
                config.MinPlayersPerGroup,
                (int)Math.Ceiling((double)distributionPlayers.Count / totalGroups)
            )
        );

        System.Diagnostics.Debug.WriteLine($"📊 Distribution: {distributionPlayers.Count} players → {totalGroups} groups");
        System.Diagnostics.Debug.WriteLine($"   Target: {playersPerGroup} players/group (Min: {config.MinPlayersPerGroup}, Max: {config.MaxPlayersPerGroup})");

        int playerIndex = 0;
        
        // Verteile Spieler basierend auf Config
        foreach (var className in config.SelectedClasses)
        {
            // ✅ FIX: Verwende GetGroupsForClass() um klassen-spezifische Regeln zu berücksichtigen
            var groupsForClass = config.GetGroupsForClass(className);
            
            // ✅ FIX: Prüfe ob Klasse übersprungen werden soll
            if (config.ClassRules.TryGetValue(className, out var classRule) && classRule.Skip)
            {
                System.Diagnostics.Debug.WriteLine($"⏭️ Skipping class: {className}");
                continue;
            }
            
            // Erstelle Gruppen für diese Klasse
            var classGroups = new List<GroupDistributionResult>();
            for (int groupNum = 1; groupNum <= groupsForClass; groupNum++)
            {
                classGroups.Add(new GroupDistributionResult
                {
                    ClassName = className,
                    GroupNumber = groupNum
                });
            }
            
            // ✅ NEU: Verteile Spieler basierend auf Mode
            if (config.Mode == DistributionMode.SnakeDraft)
            {
                // Snake Draft: 1-2-3-4-4-3-2-1
                DistributeSnakeDraft(classGroups, distributionPlayers, ref playerIndex, config);
            }
            else if (config.Mode == DistributionMode.TopHeavy)
            {
                // ✅ NEU: Top-Heavy: Stärkste zuerst (1-2-3-4, 5-6-7-8, ...)
                DistributeTopHeavy(classGroups, distributionPlayers, ref playerIndex, config);
            }
            else
            {
                // Balanced oder Random: Round-Robin
                DistributeRoundRobin(classGroups, distributionPlayers, ref playerIndex, config);
            }
            
            results.AddRange(classGroups);
        }

        // ✅ NEU: Validierung - Entferne Gruppen unter Minimum
        var validResults = results.Where(r => r.Players.Count >= config.MinPlayersPerGroup).ToList();
        
        if (validResults.Count < results.Count)
        {
            var removed = results.Count - validResults.Count;
            System.Diagnostics.Debug.WriteLine($"⚠️ Removed {removed} groups with less than {config.MinPlayersPerGroup} players");
        }

        return validResults;
    }
    
    /// <summary>
    /// ✅ NEU: Bereitet Spieler für Distribution vor basierend auf Mode
    /// </summary>
    private List<PowerScoringPlayer> PreparePlayersForDistribution(
        List<PowerScoringPlayer> players, 
        DistributionMode mode)
    {
        return mode switch
        {
            DistributionMode.Random => players.OrderBy(x => Guid.NewGuid()).ToList(),
            DistributionMode.TopHeavy => players.OrderByDescending(p => p.AverageScore).ToList(),
            _ => new List<PowerScoringPlayer>(players) // Balanced/SnakeDraft behalten Reihenfolge
        };
    }
    
    /// <summary>
    /// ✅ NEU: Snake Draft Verteilung (1-2-3-4-4-3-2-1)
    /// </summary>
    private void DistributeSnakeDraft(
        List<GroupDistributionResult> groups,
        List<PowerScoringPlayer> players,
        ref int playerIndex,
        GroupDistributionConfig config)
    {
        bool forward = true;
        int safetyCounter = 0; // ✅ FIX: Verhindere Endlosschleife
        const int maxIterations = 1000;
        
        while (playerIndex < players.Count && safetyCounter < maxIterations)
        {
            safetyCounter++;
            bool playerDistributed = false; // ✅ FIX: Track ob Spieler verteilt wurde
            
            var groupsToFill = forward ? groups : groups.AsEnumerable().Reverse();
            
            foreach (var group in groupsToFill)
            {
                if (playerIndex >= players.Count) break;
                
                var maxPlayers = config.GetMaxPlayersForGroup(group.ClassName, group.GroupNumber);
                if (group.Players.Count < maxPlayers)
                {
                    group.Players.Add(players[playerIndex]);
                    playerIndex++;
                    playerDistributed = true; // ✅ Spieler wurde verteilt
                }
            }
            
            // ✅ FIX: Wenn kein Spieler verteilt wurde, sind alle Gruppen voll → Stop!
            if (!playerDistributed)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Snake Draft stopped: All groups are full ({playerIndex}/{players.Count} players distributed)");
                break;
            }
            
            forward = !forward; // Richtung wechseln
        }
        
        // ✅ FIX: Warnung bei Safety Counter
        if (safetyCounter >= maxIterations)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Snake Draft aborted: Max iterations reached (possible infinite loop)");
        }
    }
    
    /// <summary>
    /// ✅ NEU: Round-Robin Verteilung (gleichmäßig)
    /// </summary>
    private void DistributeRoundRobin(
        List<GroupDistributionResult> groups,
        List<PowerScoringPlayer> players,
        ref int playerIndex,
        GroupDistributionConfig config)
    {
        int groupIndex = 0;
        
        while (playerIndex < players.Count)
        {
            var group = groups[groupIndex % groups.Count];
            var maxPlayers = config.GetMaxPlayersForGroup(group.ClassName, group.GroupNumber);
            
            if (group.Players.Count < maxPlayers)
            {
                group.Players.Add(players[playerIndex]);
                playerIndex++;
            }
            
            groupIndex++;
            
            // Sicherheit: Wenn alle Gruppen voll sind, stoppe
            if (groups.All(g => g.Players.Count >= config.GetMaxPlayersForGroup(g.ClassName, g.GroupNumber)))
                break;
        }
    }
    
    /// <summary>
    /// ✅ NEU: Top-Heavy Verteilung (Stärkste zuerst)
    /// Verteilt Spieler sequenziell: Gruppe 1 wird komplett gefüllt, dann Gruppe 2, etc.
    /// Resultat: Gruppe 1 hat die stärksten Spieler, Gruppe 2 die nächststärksten, etc.
    /// </summary>
    private void DistributeTopHeavy(
        List<GroupDistributionResult> groups,
        List<PowerScoringPlayer> players,
        ref int playerIndex,
        GroupDistributionConfig config)
    {
        System.Diagnostics.Debug.WriteLine($"🔝 Top-Heavy distribution starting from player index {playerIndex}");
        
        // Fülle Gruppen sequenziell (nicht Round-Robin!)
        foreach (var group in groups)
        {
            if (playerIndex >= players.Count) break;
            
            var maxPlayers = config.GetMaxPlayersForGroup(group.ClassName, group.GroupNumber);
            
            System.Diagnostics.Debug.WriteLine($"   Filling {group.ClassName} - Group {group.GroupNumber} (max: {maxPlayers})");
            
            // Fülle diese Gruppe komplett bevor zur nächsten gewechselt wird
            while (group.Players.Count < maxPlayers && playerIndex < players.Count)
            {
                group.Players.Add(players[playerIndex]);
                System.Diagnostics.Debug.WriteLine($"      + Player {playerIndex + 1}: {players[playerIndex].Name} (Avg: {players[playerIndex].AverageScore:F2})");
                playerIndex++;
            }
            
            System.Diagnostics.Debug.WriteLine($"   → Group complete: {group.Players.Count} players");
        }
        
        System.Diagnostics.Debug.WriteLine($"🔝 Top-Heavy distribution complete. Players distributed: {playerIndex}");
    }
}
