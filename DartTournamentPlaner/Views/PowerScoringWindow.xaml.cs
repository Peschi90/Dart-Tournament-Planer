using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using DartTournamentPlaner.Models.PowerScore;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.PowerScore;
using DartTournamentPlaner.Services.License;
using QRCoder;
using DartTournamentPlaner.Views;

namespace DartTournamentPlaner.Views;

/// <summary>
/// PowerScoring-Fenster für die Spieler-Einteilung basierend auf Dart-Scores mit Hub-Integration
/// </summary>
public partial class PowerScoringWindow : Window
{
    private readonly PowerScoringService _powerScoringService;
    private readonly LocalizationService _localizationService;
    private readonly LicensedHubService? _hubService;
    private readonly ConfigService? _configService;
    private readonly TournamentManagementService? _tournamentManagementService; // ✅ PHASE 3
    private readonly MainWindow? _mainWindow; // ✅ PHASE 3: MainWindow Referenz
    
    // Loading-State
    private readonly ObservableCollection<ProgressStepModel> _progressSteps = new();
    private Storyboard? _spinnerAnimation;

    public PowerScoringWindow(
        PowerScoringService powerScoringService, 
        LocalizationService localizationService,
        LicensedHubService? hubService = null,
        ConfigService? configService = null,
        TournamentManagementService? tournamentManagementService = null,
        MainWindow? mainWindow = null) // ✅ PHASE 3
    {
        InitializeComponent();
        
        _powerScoringService = powerScoringService;
        _localizationService = localizationService;
        _hubService = hubService;
        _configService = configService;
        _tournamentManagementService = tournamentManagementService; // ✅ PHASE 3
        _mainWindow = mainWindow; // ✅ PHASE 3

        InitializeRuleComboBox();
        
        // ✅ FIX: Lade Tournament-ID synchron (kein Dialog)
        LoadSavedTournamentId();
        
        // ✅ FIX: Verschiebe Session-Load-Dialog zum Loaded Event
        // damit Window vollständig initialisiert ist
        this.Loaded += PowerScoringWindow_Loaded;
        
        // UI aktualisieren nach allen Ladungen
        UpdateUI();
        
        // Subscribe zu PowerScoring Updates vom Service
        _powerScoringService.PlayerScoreUpdated += OnPlayerScoreUpdated;
        
        // Subscribe zu Hub PowerScoring Messages
        if (_hubService != null)
        {
            _hubService.PowerScoringMessageReceived += OnHubPowerScoringMessageReceived;
            System.Diagnostics.Debug.WriteLine("✅ Subscribed to Hub PowerScoring messages");
        }
        
        // Synchronisiere Tournament-ID mit TournamentData wenn sie geändert wird
        TournamentIdTextBox.TextChanged += TournamentIdTextBox_TextChanged;
        
        // ✅ NEU: Übersetze UI-Elemente
        UpdateTranslations();
    }
    
    /// <summary>
    /// Aktualisiert alle UI-Texte basierend auf der aktuellen Sprache
    /// </summary>
    private void UpdateTranslations()
    {
        try
        {
            // Window Title
            Title = _localizationService.GetString("PowerScoring_Title");
            
            // Buttons
            NewSessionButton.Content = _localizationService.GetString("PowerScoring_NewSession");
            AddPlayerButton.Content = _localizationService.GetString("PowerScoring_AddPlayer");
            StartScoringButton.Content = _localizationService.GetString("PowerScoring_StartScoring");
            CompleteScoringButton.Content = _localizationService.GetString("PowerScoring_CompleteScoring");
            PrintQRCodesButton.Content = _localizationService.GetString("PowerScoring_PrintQRCodes");
            ExportButton.Content = _localizationService.GetString("PowerScoring_ExportGroups");
            GenerateIdButton.Content = _localizationService.GetString("PowerScoring_GenerateId");
            
            // Labels & TextBlocks
            if (FindName("RuleLabel") is TextBlock ruleLabel)
                ruleLabel.Text = _localizationService.GetString("PowerScoring_Rule");
            
            if (FindName("TournamentIdLabel") is TextBlock tournamentIdLabel)
                tournamentIdLabel.Text = _localizationService.GetString("PowerScoring_TournamentId");
            
            if (FindName("PlayerListLabel") is TextBlock playerListLabel)
                playerListLabel.Text = _localizationService.GetString("PowerScoring_PlayerList");
             
             // ComboBox Items übersetzen
             UpdateRuleComboBoxTranslations();
            
            System.Diagnostics.Debug.WriteLine("✅ PowerScoring translations updated");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Error updating PowerScoring translations: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Aktualisiert die Übersetzungen der Regel-ComboBox
    /// </summary>
    private void UpdateRuleComboBoxTranslations()
    {
        var items = RuleComboBox.Items.OfType<ComboBoxItem>().ToList();
        if (items.Count >= 4)
        {
            items[0].Content = _localizationService.GetString("PowerScoring_ThrowsOf3x1");
            items[1].Content = _localizationService.GetString("PowerScoring_ThrowsOf3x8");
            items[2].Content = _localizationService.GetString("PowerScoring_ThrowsOf3x10");
            items[3].Content = _localizationService.GetString("PowerScoring_ThrowsOf3x15");
        }
    }

    private void PowerScoringWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Entferne Event-Handler um nicht mehrfach aufgerufen zu werden
        this.Loaded -= PowerScoringWindow_Loaded;
        
        // Jetzt ist das Window vollständig geladen und kann als Owner für Dialoge verwendet werden
        TryLoadSavedSession();
    }
    
    /// <summary>
    /// Synchronisiert Tournament-ID mit TournamentData
    /// </summary>
    private void TournamentIdTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            var newId = TournamentIdTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(newId))
            {
                // Aktualisiere in PowerScoring Session
                _powerScoringService.SetTournamentId(newId);
                System.Diagnostics.Debug.WriteLine($"🔄 Tournament-ID updated: {newId}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Error updating Tournament-ID: {ex.Message}");
        }
    }

    private void InitializeRuleComboBox()
    {
        RuleComboBox.Items.Add(new ComboBoxItem { Content = "1 x 3 Würfe", Tag = PowerScoringRule.ThrowsOf3x1 });
        RuleComboBox.Items.Add(new ComboBoxItem { Content = "2 x 3 Würfe", Tag = PowerScoringRule.ThrowsOf3x2 });
        RuleComboBox.Items.Add(new ComboBoxItem { Content = "3 x 3 Würfe", Tag = PowerScoringRule.ThrowsOf3x3 });
        RuleComboBox.Items.Add(new ComboBoxItem { Content = "4 x 3 Würfe", Tag = PowerScoringRule.ThrowsOf3x4 });
        RuleComboBox.Items.Add(new ComboBoxItem { Content = "5 x 3 Würfe", Tag = PowerScoringRule.ThrowsOf3x5 });
        RuleComboBox.Items.Add(new ComboBoxItem { Content = "6 x 3 Würfe", Tag = PowerScoringRule.ThrowsOf3x6 });
        RuleComboBox.Items.Add(new ComboBoxItem { Content = "7 x 3 Würfe", Tag = PowerScoringRule.ThrowsOf3x7 });
        RuleComboBox.Items.Add(new ComboBoxItem { Content = "8 x 3 Würfe", Tag = PowerScoringRule.ThrowsOf3x8 });
        RuleComboBox.Items.Add(new ComboBoxItem { Content = "9 x 3 Würfe", Tag = PowerScoringRule.ThrowsOf3x9 });
        RuleComboBox.Items.Add(new ComboBoxItem { Content = "10 x 3 Würfe", Tag = PowerScoringRule.ThrowsOf3x10 });
        RuleComboBox.Items.Add(new ComboBoxItem { Content = "11 x 3 Würfe", Tag = PowerScoringRule.ThrowsOf3x11 });
        RuleComboBox.Items.Add(new ComboBoxItem { Content = "12 x 3 Würfe", Tag = PowerScoringRule.ThrowsOf3x12 });
        RuleComboBox.Items.Add(new ComboBoxItem { Content = "13 x 3 Würfe", Tag = PowerScoringRule.ThrowsOf3x13 });
        RuleComboBox.Items.Add(new ComboBoxItem { Content = "14 x 3 Würfe", Tag = PowerScoringRule.ThrowsOf3x14 });
        RuleComboBox.Items.Add(new ComboBoxItem { Content = "15 x 3 Würfe", Tag = PowerScoringRule.ThrowsOf3x15 });


        RuleComboBox.SelectedIndex = 2; // Default: 10 x 3 Würfe
    }

    private void RuleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Ignoriere Event während Initialisierung
        if (!IsLoaded) return;
        
        if (RuleComboBox.SelectedItem is ComboBoxItem item && item.Tag is PowerScoringRule rule)
        {
            var session = _powerScoringService.CurrentSession;
            
            if (session == null)
            {
                // Keine Session vorhanden - erstelle neue
                string? currentTournamentId = TournamentIdTextBox.Text.Trim();
                if (string.IsNullOrEmpty(currentTournamentId))
                {
                    currentTournamentId = null;
                }
                
                _powerScoringService.CreateNewSession(rule, currentTournamentId);
                UpdatePlayerList();
                System.Diagnostics.Debug.WriteLine($"📋 Neue Session erstellt mit Regel: {rule}");
            }
            else
            {
                // Session existiert - nur Regel ändern, Spieler behalten
                var oldRule = session.Rule;
                session.Rule = rule;
                
                // Aktualisiere NumberOfThrows für alle Spieler
                foreach (var player in session.Players)
                {
                    player.NumberOfThrows = (int)rule;
                }
                
                UpdatePlayerList();
                System.Diagnostics.Debug.WriteLine($"📋 Regel geändert: {oldRule} → {rule}, Spieler beibehalten: {session.Players.Count}");
            }
        }
    }

    private void AddPlayerButton_Click(object sender, RoutedEventArgs e)
    {
        var session = _powerScoringService.CurrentSession;
        if (session == null)
        {
            var rule = PowerScoringRule.ThrowsOf3x10;
            if (RuleComboBox.SelectedItem is ComboBoxItem item && item.Tag is PowerScoringRule selectedRule)
            {
                rule = selectedRule;
            }

            var tournamentId = TournamentIdTextBox.Text?.Trim();
            session = _powerScoringService.CreateNewSession(rule, tournamentId);
            System.Diagnostics.Debug.WriteLine($"[PowerScoring] Created session for AddPlayer dialog. Rule={rule}, TournamentId={tournamentId}");
        }
         var dialog = new PowerScoringAddPlayersDialog(session.Players)
         {
             Owner = this
         };

         if (dialog.ShowDialog() == true)
         {
             ApplyPlayerEntries(dialog.PlayerEntries, session);
             UpdatePlayerList();
         }
    }

    private void ApplyPlayerEntries(IEnumerable<PowerScoringAddPlayersDialog.PlayerEntryRow> entries, PowerScoringSession session)
    {
        var usedNames = new HashSet<string>(session.Players.Select(p => p.Name), StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Nickname) && string.IsNullOrWhiteSpace(entry.FirstName) && string.IsNullOrWhiteSpace(entry.LastName))
            {
                continue;
            }

            var displayName = GenerateDisplayName(entry, usedNames);
            if (displayName == null)
            {
                continue;
            }

            var player = _powerScoringService.AddPlayerToSession(displayName);
            if (player != null)
            {
                usedNames.Add(displayName);
            }
        }
    }

    private string? GenerateDisplayName(PowerScoringAddPlayersDialog.PlayerEntryRow entry, HashSet<string> usedNames)
    {
        if (!string.IsNullOrWhiteSpace(entry.Nickname))
        {
            var nick = entry.Nickname.Trim();
            if (usedNames.Contains(nick))
                return null;
            return nick;
        }

        var first = entry.FirstName?.Trim();
        if (string.IsNullOrWhiteSpace(first))
            return null;

        var last = entry.LastName?.Trim() ?? string.Empty;
        if (!usedNames.Contains(first))
            return first;

        for (int len = 1; len <= last.Length; len++)
        {
            var candidate = $"{first} {last.Substring(0, len)}";
            if (!usedNames.Contains(candidate))
                return candidate;
        }

        int counter = 2;
        while (true)
        {
            var candidate = $"{first} {counter}";
            if (!usedNames.Contains(candidate))
                return candidate;
            counter++;
        }
    }

    private void RemovePlayerButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is PowerScoringPlayer player)
        {
            var result = PowerScoringConfirmDialog.ShowQuestion(
                "Spieler entfernen",
                $"Möchten Sie den Spieler '{player.Name}' wirklich entfernen?",
                this);

            if (result)
            {
                _powerScoringService.RemovePlayerFromSession(player);
                UpdatePlayerList();
            }
        }
    }

    private async void StartScoringButton_Click(object sender, RoutedEventArgs e)
    {
        var session = _powerScoringService.CurrentSession;
        if (session == null || session.Players.Count == 0)
        {
            PowerScoringConfirmDialog.ShowWarning(
                _localizationService.GetString("PowerScoring_Error_NoPlayers"),
                _localizationService.GetString("PowerScoring_Error_AddPlayers"),
                this);
            return;
        }

        // Zeige Loading-Overlay
        ShowLoadingOverlay(
            "Initializing PowerScoring...",
            "Preparing PowerScoring session with Hub integration");
        
        try
        {
            // SCHRITT 1: Hole/Generiere Tournament-ID
            AddProgressStep("🔍", "Checking Tournament ID...", "#3B82F6");
            await Task.Delay(300);
            
            string? tournamentId = TournamentIdTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(tournamentId))
            {
                UpdateLoadingMessage("Generating Tournament ID...");
                
                if (_hubService?.InnerHubService != null)
                {
                    tournamentId = _hubService.InnerHubService.GenerateNewTournamentId();
                    TournamentIdTextBox.Text = tournamentId;
                    AddProgressStep("✅", $"Generated Tournament ID: {tournamentId}", "#10B981");
                    System.Diagnostics.Debug.WriteLine($"🔄 Auto-generated Tournament ID: {tournamentId}");
                }
                else
                {
                    tournamentId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                    TournamentIdTextBox.Text = tournamentId;
                    AddProgressStep("✅", $"Fallback Tournament ID: {tournamentId}", "#F59E0B");
                    System.Diagnostics.Debug.WriteLine($"🔄 Fallback Tournament ID: {tournamentId}");
                }
            }
            else
            {
                AddProgressStep("✅", $"Using Tournament ID: {tournamentId}", "#10B981");
            }
            
            _powerScoringService.SetTournamentId(tournamentId);
            await Task.Delay(300);

            // SCHRITT 2: Registriere mit Hub (wenn verfügbar)
            bool hubRegistered = false;
            if (_hubService != null && !string.IsNullOrEmpty(tournamentId))
            {
                try
                {
                    AddProgressStep("🔗", "Connecting to Tournament Hub...", "#3B82F6");
                    UpdateLoadingMessage("Registering with Tournament Hub...");
                    System.Diagnostics.Debug.WriteLine($"🔗 Registering PowerScoring with Hub: {tournamentId}");
                    
                    var registered = await _hubService.RegisterTournamentAsync(tournamentId);
                    
                    if (registered)
                    {
                        hubRegistered = true;
                        _powerScoringService.SetRegisteredWithHub(true);
                        AddProgressStep("✅", "Hub registration successful!", "#10B981");
                        
                        // SCHRITT 3: Generiere QR-Codes für alle Spieler
                        UpdateLoadingMessage("Generating QR codes for players...");
                        AddProgressStep("📱", $"Generating QR codes for {session.Players.Count} players...", "#3B82F6");
                        await Task.Delay(300);
                        
                        var hubUrl = _configService?.Config?.HubUrl ?? "http://localhost:3000";
                        _powerScoringService.GenerateQrCodeUrls(hubUrl);
                        
                        AddProgressStep("✅", $"QR codes generated for all players", "#10B981");
                        System.Diagnostics.Debug.WriteLine($"✅ PowerScoring registered with Hub");
                        System.Diagnostics.Debug.WriteLine($"📱 QR-Codes generated for {session.Players.Count} players");
                        
                        await Task.Delay(500);
                    }
                    else
                    {
                        AddProgressStep("⚠️", "Hub registration failed", "#EF4444");
                        System.Diagnostics.Debug.WriteLine($"⚠️ Hub registration failed");
                        
                        HideLoadingOverlay();
                        
                        var result = PowerScoringConfirmDialog.ShowQuestion(
                            "Hub Registration Failed",
                            "Hub registration failed. Continue without Hub integration?\n\n" +
                            "Note: Players won't be able to use QR codes for scoring.",
                            this);
                        
                        if (!result)
                        {
                            return;
                        }
                        
                        ShowLoadingOverlay("Initializing PowerScoring...", "Starting without Hub integration...");
                        AddProgressStep("ℹ️", "Continuing without Hub integration", "#F59E0B");
                    }
                }
                catch (Exception ex)
                {
                    AddProgressStep("❌", $"Hub error: {ex.Message}", "#EF4444");
                    System.Diagnostics.Debug.WriteLine($"❌ Hub registration error: {ex.Message}");
                    
                    HideLoadingOverlay();
                    
                    var result = PowerScoringConfirmDialog.ShowQuestion(
                        "Hub Registration Error",
                        $"Hub registration error: {ex.Message}\n\n" +
                        $"Continue without Hub integration?",
                        this);
                        
                    if (!result)
                    {
                        return;
                    }
                    
                    ShowLoadingOverlay("Initializing PowerScoring...", "Starting without Hub integration...");
                    AddProgressStep("ℹ️", "Continuing without Hub integration", "#F59E0B");
                }
            }
            else
            {
                AddProgressStep("ℹ️", "Hub service not available", "#F59E0B");
            }

            // SCHRITT 4: Starte Scoring
            UpdateLoadingMessage("Starting scoring session...");
            AddProgressStep("🎯", "Starting PowerScoring session...", "#3B82F6");
            await Task.Delay(300);
            
            if (!_powerScoringService.StartScoring())
            {
                HideLoadingOverlay();
                PowerScoringConfirmDialog.ShowError(
                    "Error",
                    "Scoring could not be started.",
                    this);
                return;
            }
            
            AddProgressStep("✅", "PowerScoring session started", "#10B981");

            // SCHRITT 5: UI für Scoring-Phase anpassen
            UpdateLoadingMessage("Building user interface...");
            AddProgressStep("🎨", "Building scoring interface...", "#3B82F6");
            await Task.Delay(300);
            
            SetupPanel.Visibility = Visibility.Collapsed;
            ScoringPanel.Visibility = Visibility.Visible;
            StartScoringButton.Visibility = Visibility.Collapsed;
            CompleteScoringButton.Visibility = Visibility.Visible;

            // SCHRITT 6: Baue UI mit QR-Codes (wenn Hub registriert) oder manueller Eingabe
            if (hubRegistered)
            {
                BuildScoringUIWithQrCodes();
                AddProgressStep("✅", "QR code interface ready", "#10B981");
                System.Diagnostics.Debug.WriteLine("🎯 Scoring-Phase gestartet mit QR-Codes");
                
                // Zeige Print Button
                PrintQRCodesButton.Visibility = Visibility.Visible;
            }
            else
            {
                BuildScoringUI();
                AddProgressStep("✅", "Manual scoring interface ready", "#10B981");
                System.Diagnostics.Debug.WriteLine("🎯 Scoring-Phase gestartet (Hub-Modus erforderlich)");
            }
            
            await Task.Delay(500);
            
            // Final Success
            UpdateLoadingMessage("PowerScoring ready!");
            AddProgressStep("🎉", "PowerScoring is ready to use!", "#10B981");
            await Task.Delay(1000);
            
            HideLoadingOverlay();
            
            // Zeige Success-Nachricht nur wenn Hub registriert
            if (hubRegistered)
            {
                PowerScoringConfirmDialog.ShowSuccess(
                    "PowerScoring Ready",
                    $"PowerScoring session ready!\n\n" +
                    $"Tournament ID: {tournamentId}\n\n" +
                    $"Players can now scan their QR codes to enter scores.",
                    this);
            }
        }
        catch (Exception ex)
        {
            HideLoadingOverlay();
            
            System.Diagnostics.Debug.WriteLine($"❌ Error starting PowerScoring: {ex.Message}");
            PowerScoringConfirmDialog.ShowError(
                "Error",
                $"Error starting PowerScoring:\n\n{ex.Message}",
                this);
        }
    }
    
    private void CompleteScoringButton_Click(object sender, RoutedEventArgs e)
    {
        var session = _powerScoringService.CurrentSession;
        
        if (session == null)
            return;

        // Prüfe ob alle Spieler gescored wurden
        if (!session.Players.All(p => p.IsScored))
        {
            var result = PowerScoringConfirmDialog.ShowQuestion(
                "Unvollständige Scores",
                "Nicht alle Spieler haben einen Score. Möchten Sie trotzdem fortfahren?",
                this);

            if (!result)
                return;
        }

        if (!_powerScoringService.CompleteSession())
        {
            PowerScoringConfirmDialog.ShowError(
                "Fehler",
                "Session konnte nicht abgeschlossen werden.",
                this);
            return;
        }

        ShowResults();
    }

    private void ShowResults()
    {
        var rankedPlayers = _powerScoringService.GetRankedPlayers();
        
        // ✅ NEU: Verwende ViewModel statt anonymes Objekt um Player-Referenz zu behalten
        var rankedData = rankedPlayers.Select((p, index) => new PowerScoringResultViewModel
        {
            Rank = index + 1,
            Player = p,
            Name = p.Name,
            TotalScore = p.TotalScore,
            AverageScore = p.AverageScore
        }).ToList();

        ResultsDataGrid.ItemsSource = rankedData;

        // ✅ Setup ausblenden, Results anzeigen
        SetupPanel.Visibility = Visibility.Collapsed;
        ScoringPanel.Visibility = Visibility.Collapsed;
        ResultsPanel.Visibility = Visibility.Visible;
        CompleteScoringButton.Visibility = Visibility.Collapsed;
        ExportButton.Visibility = Visibility.Visible;

        System.Diagnostics.Debug.WriteLine("📊 Ergebnisse angezeigt");
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        // ✅ PHASE 3: Zeige erweiterten Dialog zur Gruppeneinteilung mit Tournament-Integration
        var dialog = new PowerScoringAdvancedGroupDialog(
            _powerScoringService, 
            _localizationService,
            _tournamentManagementService, // ✅ PHASE 3: Für Tournament-Erstellung
            this,                          // ✅ PHASE 3: PowerScoringWindow als Parent
            _mainWindow);                  // ✅ PHASE 3: MainWindow für UI-Refresh
        dialog.Owner = this;
        dialog.ShowDialog();
    }
    
    /// <summary>
    /// ✅ NEU: Zeigt Player-Details vom Kontextmenü
    /// </summary>
    private void ShowPlayerDetailsFromContextMenu_Click(object sender, RoutedEventArgs e)
    {
        if (ResultsDataGrid.SelectedItem is PowerScoringResultViewModel viewModel)
        {
            ShowPlayerDetails(viewModel.Player);
        }
    }
    
    /// <summary>
    /// ✅ NEU: Kopiert Spielername in die Zwischenablage
    /// </summary>
    private void CopyPlayerName_Click(object sender, RoutedEventArgs e)
    {
        if (ResultsDataGrid.SelectedItem is PowerScoringResultViewModel viewModel)
        {
            try
            {
                Clipboard.SetText(viewModel.Name);
                System.Diagnostics.Debug.WriteLine($"📋 Player name copied: {viewModel.Name}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error copying name: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Druckt QR-Codes für alle Spieler
    /// </summary>
    private void PrintQRCodesButton_Click(object sender, RoutedEventArgs e)
    {
        var session = _powerScoringService.CurrentSession;
        if (session == null || session.Players.Count == 0)
        {
            PowerScoringConfirmDialog.ShowWarning(
                _localizationService.GetString("PowerScoring_Error_NoPlayers"),
                _localizationService.GetString("PowerScoring_Warning_NoPlayersToPrint"),
                this);
            return;
        }
        
        try
        {
            // TODO: Implement QR Code printing
            PowerScoringConfirmDialog.ShowInformation(
                _localizationService.GetString("PowerScoring_Info_PrintQRCodes"),
                _localizationService.GetString("PowerScoring_Info_PrintFeatureComingSoon"),
                this);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error printing QR codes: {ex.Message}");
            PowerScoringConfirmDialog.ShowError(
                _localizationService.GetString("Error"),
                $"{_localizationService.GetString("Error")}: {ex.Message}",
                this);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_powerScoringService.CurrentSession?.Status == PowerScoringStatus.Scoring)
        {
            var result = PowerScoringConfirmDialog.ShowQuestion(
                "Scoring nicht abgeschlossen",
                "Das Scoring ist noch nicht abgeschlossen. Die Session wird automatisch gespeichert.\n\n" +
                "Möchten Sie wirklich schließen?",
                this);

            if (!result)
                return;
        }
        
        // Session wird automatisch gespeichert durch Auto-Save
        Close();
    }
    
    /// <summary>
    /// Erstellt eine neue Session (löscht die aktuelle)
    /// </summary>
    private void NewSessionButton_Click(object sender, RoutedEventArgs e)
    {
        if (_powerScoringService.CurrentSession != null)
        {
            var result = PowerScoringConfirmDialog.ShowQuestion(
                _localizationService.GetString("PowerScoring_Confirm_NewSession_Title"),
                _localizationService.GetString("PowerScoring_Confirm_NewSession_Message"),
                this);
            
            if (!result)
                return;
        }
        
        // ✅ FIX: Speichere Tournament-ID bevor Session gelöscht wird
        string? savedTournamentId = TournamentIdTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(savedTournamentId))
        {
            savedTournamentId = null;
        }
        
        // Lösche alte Session
        _powerScoringService.DeleteSavedSession();
        
        // Erstelle neue Session MIT der gespeicherten Tournament-ID
        if (RuleComboBox.SelectedItem is ComboBoxItem item && item.Tag is PowerScoringRule rule)
        {
            _powerScoringService.CreateNewSession(rule, savedTournamentId);
            
            // ✅ FIX: Stelle sicher dass Tournament-ID in TextBox bleibt
            if (!string.IsNullOrEmpty(savedTournamentId))
            {
                TournamentIdTextBox.Text = savedTournamentId;
            }
            
            UpdatePlayerList();
            UpdateUI();
            
            PowerScoringConfirmDialog.ShowSuccess(
                _localizationService.GetString("PowerScoring_Success_NewSession"),
                _localizationService.GetString("PowerScoring_Success_NewSessionCreated"),
                this);
        }
    }

    private void UpdatePlayerList()
    {
        if (_powerScoringService.CurrentSession != null)
        {
            PlayerListItems.ItemsSource = null;
            PlayerListItems.ItemsSource = _powerScoringService.CurrentSession.Players;
        }
    }

    private void UpdateUI()
    {
        SetupPanel.Visibility = Visibility.Visible;
        ScoringPanel.Visibility = Visibility.Collapsed;
        ResultsPanel.Visibility = Visibility.Collapsed;
        StartScoringButton.Visibility = Visibility.Visible;
        CompleteScoringButton.Visibility = Visibility.Collapsed;
        ExportButton.Visibility = Visibility.Collapsed;
    }

    private void LoadSavedTournamentId()
    {
        try
        {
            // Hole Tournament-ID aus PowerScoring Session
            var session = _powerScoringService.CurrentSession;
            if (session != null && !string.IsNullOrWhiteSpace(session.TournamentId))
            {
                TournamentIdTextBox.Text = session.TournamentId;
                System.Diagnostics.Debug.WriteLine($"📂 Loaded Tournament ID from PowerScoring Session: {session.TournamentId}");
                return;
            }
            
            System.Diagnostics.Debug.WriteLine($"ℹ️ No saved Tournament ID found");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Could not load saved Tournament ID: {ex.Message}");
        }
    }

    private void GenerateId_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_hubService?.InnerHubService != null)
            {
                var newId = _hubService.InnerHubService.GenerateNewTournamentId();
                TournamentIdTextBox.Text = newId;
                
                if (_powerScoringService.CurrentSession != null)
                {
                    _powerScoringService.SetTournamentId(newId);
                }
                
                System.Diagnostics.Debug.WriteLine($"🔄 Generated new Tournament ID: {newId}");
            }
            else
            {
                var newId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                TournamentIdTextBox.Text = newId;
                
                if (_powerScoringService.CurrentSession != null)
                {
                    _powerScoringService.SetTournamentId(newId);
                }
                
                System.Diagnostics.Debug.WriteLine($"🔄 Generated fallback Tournament ID: {newId}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error generating new ID: {ex.Message}");
            
            var errorTitle = _localizationService.GetString("Error") ?? "Fehler";
            var errorMessage = $"{_localizationService.GetString("GenerateIdError") ?? "Fehler beim Generieren der ID"}: {ex.Message}";
            
            PowerScoringConfirmDialog.ShowError(errorTitle, errorMessage, this);
        }
    }

    /// <summary>
    /// Versucht eine gespeicherte Session zu laden
    /// </summary>
    private void TryLoadSavedSession()
    {
        try
        {
            if (_powerScoringService.HasSavedSession())
            {
                var result = PowerScoringConfirmDialog.ShowQuestion(
                    _localizationService.GetString("PowerScoring_Confirm_SavedSession_Title"),
                    _localizationService.GetString("PowerScoring_Confirm_SavedSession_Message"),
                    this);
                
                if (result)
                {
                    var session = _powerScoringService.LoadSession();
                    if (session != null)
                    {
                        // ✅ FIX: Aktualisiere Tournament-ID TextBox VOR UpdatePlayerList
                        if (!string.IsNullOrEmpty(session.TournamentId))
                        {
                            TournamentIdTextBox.Text = session.TournamentId;
                            System.Diagnostics.Debug.WriteLine($"📋 Tournament-ID from session: {session.TournamentId}");
                        }
                        
                        // Aktualisiere UI basierend auf geladener Session
                        if (session.Rule == PowerScoringRule.ThrowsOf3x1)
                            RuleComboBox.SelectedIndex = 0;
                        else if (session.Rule == PowerScoringRule.ThrowsOf3x8)
                            RuleComboBox.SelectedIndex = 1;
                        else if (session.Rule == PowerScoringRule.ThrowsOf3x10)
                            RuleComboBox.SelectedIndex = 2;
                        else if (session.Rule == PowerScoringRule.ThrowsOf3x15)
                            RuleComboBox.SelectedIndex = 3;
                        
                        // ✅ FIX: Jetzt UpdatePlayerList mit bereits gesetzter Tournament-ID
                        UpdatePlayerList();
                        
                        System.Diagnostics.Debug.WriteLine($"📋 Session loaded with {session.Players.Count} players");
                        
                        // Falls Session schon im Scoring-Modus war
                        if (session.Status == PowerScoringStatus.Scoring)
                        {
                            SetupPanel.Visibility = Visibility.Collapsed;
                            ScoringPanel.Visibility = Visibility.Visible;
                            StartScoringButton.Visibility = Visibility.Collapsed;
                            CompleteScoringButton.Visibility = Visibility.Visible;
                            
                            BuildScoringUIWithQrCodes();
                        }
                        // Falls Session completed war
                        else if (session.Status == PowerScoringStatus.Completed)
                        {
                            ShowResults();
                        }
                        
                        PowerScoringConfirmDialog.ShowSuccess(
                            _localizationService.GetString("PowerScoring_Success_SessionLoaded"),
                            $"{_localizationService.GetString("PowerScoring_Success_SessionLoaded")}\n\n" +
                            $"{_localizationService.GetString("PowerScoring_Player")}: {session.Players.Count}\n" +
                            $"{_localizationService.GetString("PowerScoring_Rule")}: {session.Rule}\n" +
                            $"Status: {session.Status}",
                            this);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ Session konnte nicht geladen werden (null)");
                    }
                }
                else
                {
                    // User wollte Session nicht fortsetzen - lösche sie
                    _powerScoringService.DeleteSavedSession();
                    System.Diagnostics.Debug.WriteLine("🗑️ Saved session deleted by user choice");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading saved session: {ex.Message}");
            
            PowerScoringConfirmDialog.ShowError(
                _localizationService.GetString("Error"),
                $"{_localizationService.GetString("Error")}: {ex.Message}",
                this);
        }
    }

    #region UI Building Methods

    private void BuildScoringUIWithQrCodes()
    {
        ScoringItems.Items.Clear();
        var session = _powerScoringService.CurrentSession;
        
        if (session == null) return;

        foreach (var player in session.Players)
        {
            var playerPanel = CreatePlayerQrCodePanel(player);
            ScoringItems.Items.Add(playerPanel);
        }
    }
    
    private void BuildScoringUI()
    {
        ScoringItems.Items.Clear();
        
        var infoPanel = new Border
        {
            Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FEF3C7")!,
            BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#F59E0B")!,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 20, 0, 0)
        };

        var stack = new StackPanel();
        
        var titleBlock = new TextBlock
        {
            Text = "⚠️ Hub Integration Required",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#92400E")!,
            Margin = new Thickness(0, 0, 0, 10)
        };
        stack.Children.Add(titleBlock);

        var messageBlock = new TextBlock
        {
            Text = "PowerScoring requires Tournament Hub integration for QR code-based scoring.\n\n" +
                   "Manual score entry is not available in this version. Please ensure:\n" +
                   "• Tournament Hub is running and accessible\n" +
                   "• Hub URL is configured in Settings\n" +
                   "• Network connectivity is available",
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#92400E")!
        };
        stack.Children.Add(messageBlock);

        infoPanel.Child = stack;
        ScoringItems.Items.Add(infoPanel);
    }

    private Border CreatePlayerQrCodePanel(PowerScoringPlayer player)
    {
        var border = new Border
        {
            Background = (SolidColorBrush)Application.Current.Resources["BackgroundBrush"],
            BorderBrush = (SolidColorBrush)Application.Current.Resources["BorderBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(15),
            Margin = new Thickness(0, 0, 0, 10)
        };

        var mainGrid = new Grid();
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // LINKE SPALTE: QR-CODE
        var leftStack = new StackPanel { Margin = new Thickness(0, 0, 20, 0) };
        
        var nameBlock = new TextBlock
        {
            Text = $"Player: {player.Name}",
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        leftStack.Children.Add(nameBlock);

        if (!string.IsNullOrEmpty(player.QrCodeUrl))
        {
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(player.QrCodeUrl, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                
                byte[] qrCodeBytes = qrCode.GetGraphic(20);
                
                var bitmapImage = new BitmapImage();
                using (var stream = new System.IO.MemoryStream(qrCodeBytes))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = stream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                }

                var qrImage = new System.Windows.Controls.Image
                {
                    Source = bitmapImage,
                    Width = 200,
                    Height = 200,
                    Margin = new Thickness(0, 0, 0, 10),
                    Stretch = System.Windows.Media.Stretch.Uniform
                };
                
                leftStack.Children.Add(qrImage);
                
                System.Diagnostics.Debug.WriteLine($"✅ QR-Code generated for {player.Name}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error generating QR-Code: {ex.Message}");
                
                var errorBorder = new Border
                {
                    Width = 200,
                    Height = 200,
                    Background = System.Windows.Media.Brushes.LightGray,
                    BorderBrush = System.Windows.Media.Brushes.Red,
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var errorText = new TextBlock
                {
                    Text = "QR CODE\nGENERATION\nERROR",
                    FontSize = 14,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = System.Windows.Media.Brushes.Red
                };

                errorBorder.Child = errorText;
                leftStack.Children.Add(errorBorder);
            }
            
            var urlLabel = new TextBlock
            {
                Text = "Scan URL:",
                FontWeight = FontWeights.SemiBold,
                FontSize = 11,
                Margin = new Thickness(0, 5, 0, 2)
            };
            leftStack.Children.Add(urlLabel);

            var urlTextBox = new TextBox
            {
                Text = player.QrCodeUrl,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 9,
                Background = (SolidColorBrush)Application.Current.Resources["SurfaceBrush"],
                BorderBrush = (SolidColorBrush)Application.Current.Resources["BorderBrush"],
                Padding = new Thickness(5),
                MaxWidth = 200
            };
            leftStack.Children.Add(urlTextBox);

            var copyButton = new Button
            {
                Content = "Copy URL",
                Margin = new Thickness(0, 5, 0, 0),
                Padding = new Thickness(8, 4, 8, 4),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            copyButton.Click += (s, e) =>
            {
                try
                {
                    Clipboard.SetText(player.QrCodeUrl);
                    // ✅ Kein Bestätigungsdialog mehr - URL wird leise kopiert
                    System.Diagnostics.Debug.WriteLine($"📋 URL copied to clipboard for {player.Name}");
                }
                catch (Exception ex)
                {
                    PowerScoringConfirmDialog.ShowError(
                        "Error",
                        $"Error copying URL: {ex.Message}",
                        this);
                }
            };
            leftStack.Children.Add(copyButton);
        }

        Grid.SetColumn(leftStack, 0);
        mainGrid.Children.Add(leftStack);

        // RECHTE SPALTE: LIVE SCORES
        var rightStack = new StackPanel();
        
        var scoreTitle = new TextBlock
        {
            Text = "Live Score:",
            FontWeight = FontWeights.SemiBold,
            FontSize = 16,
            Margin = new Thickness(0, 0, 0, 10)
        };
        rightStack.Children.Add(scoreTitle);

        // Score Display (wird via Binding aktualisiert)
        var scoreDisplay = new TextBlock
        {
            Text = player.IsScored ? 
                $"Total: {player.TotalScore}\nAverage: {player.AverageScore:F2}\nRounds: {player.History.Count}" : 
                "Waiting for scores...",
            FontSize = 14,
            Margin = new Thickness(0, 0, 0, 10),
            Tag = player.PlayerId
        };
        rightStack.Children.Add(scoreDisplay);
        
        // Erweiterte Statistiken (nur sichtbar wenn Daten vorhanden)
        if (player.HighestThrow > 0 || player.TotalDarts > 0)
        {
            var statsTitle = new TextBlock
            {
                Text = "Statistics:",
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                Margin = new Thickness(0, 10, 0, 5)
            };
            rightStack.Children.Add(statsTitle);
            
            var statsDisplay = new TextBlock
            {
                Text = $"Highest: {player.HighestThrow}\n" +
                       $"Total Darts: {player.TotalDarts}",
                FontSize = 12,
                Foreground = (SolidColorBrush)Application.Current.Resources["SecondaryTextBrush"],
                Margin = new Thickness(0, 0, 0, 10),
                Tag = $"stats_{player.PlayerId}"
            };
            rightStack.Children.Add(statsDisplay);
        }

        // Status Indicator
        var statusBorder = new Border
        {
            Background = player.IsScored ? 
                System.Windows.Media.Brushes.LightGreen : 
                System.Windows.Media.Brushes.LightGray,
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(10, 5, 10, 5),
            HorizontalAlignment = HorizontalAlignment.Left
        };

        var statusText = new TextBlock
        {
            Text = player.IsScored ? "Completed" : "Waiting...",
            FontWeight = FontWeights.SemiBold,
            Tag = $"status_{player.PlayerId}"
        };

        statusBorder.Child = statusText;
        rightStack.Children.Add(statusBorder);
        
        // Details Button für History-Anzeige
        if (player.History.Count > 0)
        {
            // ✅ FIX: Erstelle lokale Kopie für Closure (Sicherheit)
            var playerForButton = player;
            
            var detailsButton = new Button
            {
                Content = "Show Details",
                Margin = new Thickness(0, 10, 0, 0),
                Padding = new Thickness(8, 4, 8, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Tag = $"details_{player.PlayerId}"  // ✅ NEU: Tag für Identifikation
            };
            detailsButton.Click += (s, e) => ShowPlayerDetails(playerForButton);
            rightStack.Children.Add(detailsButton);
        }

        Grid.SetColumn(rightStack, 1);
        mainGrid.Children.Add(rightStack);

        border.Child = mainGrid;
        return border;
    }
    
    /// <summary>
    /// Zeigt detaillierte Spieler-Statistiken und Wurf-Historie
    /// </summary>
    private void ShowPlayerDetails(PowerScoringPlayer player)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"📊 Showing details for {player.Name}: {player.History.Count} rounds");
            
            // ✅ NEU: Verwende modernen Dialog statt einfachem Text-Dialog
            PowerScoringPlayerDetailsDialog.Show(player, this);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error showing player details: {ex.Message}");
            PowerScoringConfirmDialog.ShowError(
                "Error",
                $"Error displaying player details:\n\n{ex.Message}",
                this);
        }
    }

    #endregion

    #region Event Handlers

    private void OnPlayerScoreUpdated(object? sender, PowerScoringPlayer player)
    {
        Dispatcher.Invoke(() =>
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"📊 [LOCAL-UPDATE] Player score updated: {player.Name} - Total: {player.TotalScore}, Avg: {player.AverageScore:F2}");
                UpdatePlayerScoreInUI(player);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error updating player score UI: {ex.Message}");
            }
        });
    }

    private void OnHubPowerScoringMessageReceived(object? sender, Services.PowerScore.PowerScoringHubMessage message)
    {
        Dispatcher.Invoke(() =>
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"📨 [HUB-MESSAGE] PowerScoring message received: Type={message.Type}, Player={message.PlayerName}");
                System.Diagnostics.Debug.WriteLine($"   ParticipantId: {message.ParticipantId}");
                System.Diagnostics.Debug.WriteLine($"   TotalScore: {message.TotalScore}, Average: {message.Average:F2}");
                
                var success = _powerScoringService.ProcessPowerScoringResult(message);
                
                if (success)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ PowerScoring message processed successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Failed to process PowerScoring message");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error handling Hub PowerScoring message: {ex.Message}");
            }
        });
    }

    private void UpdatePlayerScoreInUI(PowerScoringPlayer player)
    {
        try
        {
            foreach (var item in ScoringItems.Items)
            {
                if (item is Border border && border.Child is Grid grid)
                {
                    foreach (var child in FindVisualChildren<TextBlock>(grid))
                    {
                        // Score Display
                        if (child.Tag is Guid playerId && playerId == player.PlayerId)
                        {
                            child.Text = $"Total: {player.TotalScore}\n" +
                                       $"Average: {player.AverageScore:F2}\n" +
                                       $"Rounds: {player.History.Count}";
                            System.Diagnostics.Debug.WriteLine($"   ✅ Updated score display for {player.Name}");
                        }
                        
                        // Stats Display
                        if (child.Tag is string tagStr && tagStr == $"stats_{player.PlayerId}")
                        {
                            child.Text = $"Highest: {player.HighestThrow}\n" +
                                       $"Total Darts: {player.TotalDarts}";
                            System.Diagnostics.Debug.WriteLine($"   ✅ Updated stats for {player.Name}");
                        }
                        
                        // Status Text
                        if (child.Tag is string statusTag && statusTag == $"status_{player.PlayerId}")
                        {
                            child.Text = player.IsScored ? "Completed" : $"Round {player.History.Count}";
                                
                            // Aktualisiere auch die Farbe des Status Borders
                            if (child.Parent is Border statusBorder)
                            {
                                statusBorder.Background = player.IsScored ? 
                                    System.Windows.Media.Brushes.LightGreen : 
                                    System.Windows.Media.Brushes.LightYellow;
                            }
                            

                            System.Diagnostics.Debug.WriteLine($"   ✅ Updated status for {player.Name}");
                        }
                    }
                    
                    // Aktualisiere oder füge Details-Button hinzu wenn History vorhanden
                    if (player.History.Count > 0)
                    {
                        var hasDetailsButton = false;
                        foreach (var child in FindVisualChildren<Button>(grid))
                        {
                            if (child.Content?.ToString() == "Show Details")
                            {
                                hasDetailsButton = true;
                                break;
                            }
                        }
                        
                        // ✅ FIX: Erstelle lokale Kopie der Player-Referenz für Closure
                        var playerForButton = player;
                        
                        if (!hasDetailsButton)
                        {
                            // Füge Details-Button hinzu
                            var rightStack = grid.Children.OfType<StackPanel>().Skip(1).FirstOrDefault();
                            if (rightStack != null)
                            {
                                var detailsButton = new Button
                                {
                                    Content = "Show Details",
                                    Margin = new Thickness(0, 10, 0, 0),
                                    Padding = new Thickness(8, 4, 8, 4),
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                    Tag = $"details_{player.PlayerId}"  // ✅ NEU: Tag für Identifikation
                                };
                                detailsButton.Click += (s, e) => ShowPlayerDetails(playerForButton);
                                rightStack.Children.Add(detailsButton);
                                
                                System.Diagnostics.Debug.WriteLine($"   ✅ Added details button for {playerForButton.Name}");
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error updating UI: {ex.Message}");
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj != null)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T typedChild)
                {
                    yield return typedChild;
                }

                foreach (var childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (_powerScoringService != null)
        {
            _powerScoringService.PlayerScoreUpdated -= OnPlayerScoreUpdated;
        }
        
        if (_hubService != null)
        {
            _hubService.PowerScoringMessageReceived -= OnHubPowerScoringMessageReceived;
        }
        
        _spinnerAnimation?.Stop();
        
        base.OnClosing(e);
    }

    #endregion

    #region Loading Overlay

    private void ShowLoadingOverlay(string title, string message)
    {
        Dispatcher.Invoke(() =>
        {
            LoadingTitle.Text = title;
            LoadingMessage.Text = message;
            _progressSteps.Clear();
            ProgressSteps.ItemsSource = _progressSteps;
            
            LoadingOverlay.Visibility = Visibility.Visible;
            StartSpinnerAnimation();
            
            System.Diagnostics.Debug.WriteLine($"🔄 Loading overlay shown: {title}");
        });
    }
    
    private void HideLoadingOverlay()
    {
        Dispatcher.Invoke(() =>
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            _spinnerAnimation?.Stop();
            
            System.Diagnostics.Debug.WriteLine($"✅ Loading overlay hidden");
        });
    }
    
    private void AddProgressStep(string icon, string message, string color)
    {
        Dispatcher.Invoke(() =>
        {
            _progressSteps.Add(new ProgressStepModel
            {
                Icon = icon,
                Message = message,
                Color = (SolidColorBrush)new BrushConverter().ConvertFromString(color)!,
            });
        });
    }
    
    private void UpdateLoadingMessage(string message)
    {
        Dispatcher.Invoke(() =>
        {
            LoadingMessage.Text = message;
        });
    }
    
    private void StartSpinnerAnimation()
    {
        if (SpinnerRotation != null)
        {
            _spinnerAnimation = new Storyboard();
            var rotation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever
            };
            
            Storyboard.SetTarget(rotation, SpinnerRotation);
            Storyboard.SetTargetProperty(rotation, new PropertyPath(RotateTransform.AngleProperty));
            
            _spinnerAnimation.Children.Add(rotation);
            _spinnerAnimation.Begin();
        }
    }

    #endregion
}

/// <summary>
/// Model für Progress-Steps im Loading-Overlay
/// </summary>
public class ProgressStepModel
{
    public string Icon { get; set; } = "";
    public string Message { get; set; } = "";
    public SolidColorBrush Color { get; set; } = Brushes.Gray;
}

public class PowerScoringResultViewModel
{
    public int Rank { get; set; }
    public PowerScoringPlayer Player { get; set; } = null!;
    public string Name { get; set; } = "";
    public double TotalScore { get; set; }
    public double AverageScore { get; set; }
}
