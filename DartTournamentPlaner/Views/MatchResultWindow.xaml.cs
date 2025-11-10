using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using QRCoder;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace DartTournamentPlaner.Views;

public partial class MatchResultWindow : Window, INotifyPropertyChanged
{
    private readonly Match _match;
    private readonly GameRules _gameRules;
    private readonly LocalizationService _localizationService;
    private readonly HubIntegrationService? _hubService;
    private readonly string? _tournamentId;  // ⭐ NEU: Tournament-ID
    private string? _matchPageUrl;
    
    public MatchResultWindow(Match match, GameRules gameRules, LocalizationService localizationService, HubIntegrationService? hubService = null, string? tournamentId = null)
    {
        _match = match;
        _gameRules = gameRules;
        _localizationService = localizationService;
        _hubService = hubService;
     _tournamentId = tournamentId;  // ⭐ NEU
        
        // ✅ IMPROVED DEBUG: Detaillierte Constructor-Informationen
        System.Diagnostics.Debug.WriteLine($"🎯 [MatchResultWindow] Constructor called with regular Match");
        System.Diagnostics.Debug.WriteLine($"🎯 [MatchResultWindow]   Match ID: {_match.Id}");
        System.Diagnostics.Debug.WriteLine($"🎯 [MatchResultWindow]   Match UUID: {_match.UniqueId ?? "null"}");
        System.Diagnostics.Debug.WriteLine($"🎯 [MatchResultWindow]   Tournament ID: {_tournamentId ?? "null"}");  // ⭐ NEU
   System.Diagnostics.Debug.WriteLine($"🎯 [MatchResultWindow]   HubService provided: {_hubService != null}");
     System.Diagnostics.Debug.WriteLine($"🎯 [MatchResultWindow]   HubService registered: {_hubService?.IsRegisteredWithHub}");
        
    InitializeComponent();
        InitializeUI();
    InitializeHubIntegration();
      UpdateTranslations();
    }

    /// <summary>
    /// Constructor for KnockoutMatch with round-specific rules
    /// </summary>
    /// <param name="knockoutMatch">The knockout match</param>
    /// <param name="roundRules">Round-specific rules for this match</param>
    /// <param name="baseGameRules">Base game rules</param>
    /// <param name="localizationService">Localization service</param>
    /// <param name="hubService">Hub integration service</param>
    /// <param name="tournamentId">Tournament-ID for QR codes</param>
    public MatchResultWindow(KnockoutMatch knockoutMatch, RoundRules roundRules, GameRules baseGameRules, LocalizationService localizationService, HubIntegrationService? hubService = null, string? tournamentId = null)
  {
        // Convert KnockoutMatch to Match
        _match = new Match
 {
     Id = knockoutMatch.Id,
            UniqueId = knockoutMatch.UniqueId, // WICHTIG: UUID übertragen
            Player1 = knockoutMatch.Player1,
       Player2 = knockoutMatch.Player2,
         Player1Sets = knockoutMatch.Player1Sets,
        Player2Sets = knockoutMatch.Player2Sets,
            Player1Legs = knockoutMatch.Player1Legs,
            Player2Legs = knockoutMatch.Player2Legs,
Winner = knockoutMatch.Winner,
   Status = knockoutMatch.Status,
        Notes = knockoutMatch.Notes,
     CreatedAt = knockoutMatch.CreatedAt,
          FinishedAt = knockoutMatch.FinishedAt
        };

        // Create temporary GameRules with round-specific settings
        // WICHTIG: PlayWithSets wird NUR durch rundenspezifische SetsToWin bestimmt
        _gameRules = new GameRules
        {
            GameMode = baseGameRules.GameMode,
  FinishMode = baseGameRules.FinishMode,
            PlayWithSets = roundRules.SetsToWin > 0, // Sets nur wenn rundenspezifisch > 0
            SetsToWin = roundRules.SetsToWin,
       LegsToWin = roundRules.LegsToWin,
         LegsPerSet = roundRules.LegsPerSet
        };

    _localizationService = localizationService;
        _hubService = hubService;
  _tournamentId = tournamentId;  // ⭐ NEU
      
     InitializeComponent();
        InitializeUI();
        InitializeHubIntegration();
        UpdateTranslations();
    }

    private void InitializeUI()
    {
        // Set initial values
        MatchInfoBlock.Text = $"{_match.Player1?.Name ?? "Player 1"} vs {_match.Player2?.Name ?? "Player 2"}";
        
        // HINZUGEFÜGT: UUID-Anzeige für Debugging/Hub-Integration
        MatchUuidBlock.Text = $"UUID: {_match.UniqueId}";
        
        // Display game rules info
        var setsToWin = _gameRules.SetsToWin;
        var legsToWin = _gameRules.LegsToWin;
        var legsPerSet = _gameRules.LegsPerSet;
        
        // VEREINFACHT: Sets werden nur angezeigt wenn PlayWithSets = true
        // (Das wird bereits korrekt in den Constructors gesetzt)
        bool requiresSets = _gameRules.PlayWithSets;
        
        var rulesText = requiresSets 
            ? $"{_gameRules.GameMode} {_gameRules.FinishMode}, First to {setsToWin} Sets ({legsPerSet} Legs per Set)"
            : $"{_gameRules.GameMode} {_gameRules.FinishMode}, First to {legsToWin} Legs";
        
        GameRulesBlock.Text = rulesText;
        
        // Show/hide sets section based on whether sets are required
        SetsSection.Visibility = requiresSets ? Visibility.Visible : Visibility.Collapsed;
        
        System.Diagnostics.Debug.WriteLine($"MatchResultWindow.InitializeUI: PlayWithSets={_gameRules.PlayWithSets}, SetsToWin={setsToWin}, requiresSets={requiresSets}");
        System.Diagnostics.Debug.WriteLine($"MatchResultWindow.InitializeUI: SetsSection.Visibility={SetsSection.Visibility}");
        
        // Update player names
        Player1NameSets.Text = _match.Player1?.Name ?? "Player 1";
        Player2NameSets.Text = _match.Player2?.Name ?? "Player 2";
        Player1NameLegs.Text = _match.Player1?.Name ?? "Player 1";
        Player2NameLegs.Text = _match.Player2?.Name ?? "Player 2";
        
        // Set initial values
        Player1SetsTextBox.Text = _match.Player1Sets.ToString();
        Player2SetsTextBox.Text = _match.Player2Sets.ToString();
        Player1LegsTextBox.Text = _match.Player1Legs.ToString();
        Player2LegsTextBox.Text = _match.Player2Legs.ToString();
        NotesTextBox.Text = _match.Notes;
        
        // Ensure SaveButton starts disabled
        SaveButton.IsEnabled = false;
        
        // Subscribe to text changes to update winner info
        Player1SetsTextBox.TextChanged += (s, e) => ValidateAndUpdateWinner();
        Player2SetsTextBox.TextChanged += (s, e) => ValidateAndUpdateWinner();
        Player1LegsTextBox.TextChanged += (s, e) => ValidateAndUpdateWinner();
        Player2LegsTextBox.TextChanged += (s, e) => ValidateAndUpdateWinner();
        
        // Initial validation - this will properly set the SaveButton state
        ValidateAndUpdateWinner();
    }

    private void UpdateTranslations()
    {
        Title = _localizationService.GetString("EnterMatchResult");
        SaveButton.Content = _localizationService.GetString("SaveResult");
        CancelButton.Content = _localizationService.GetString("Cancel");
        NotesLabel.Text = _localizationService.GetString("Notes") + ":";
        LegsHeaderText.Text = _localizationService.GetString("Legs").ToUpper();
        
        // HINZUGEFÜGT: UUID-Anzeige lokalisiert
        MatchUuidBlock.Text = $"UUID: {_match.UniqueId}";
        
        // Update header title dynamically
        if (FindName("HeaderTitle") is TextBlock headerTitle)
        {
            headerTitle.Text = _localizationService.GetString("EnterMatchResult");
        }
    }

    #region NumericUpDown Event Handlers
    
    private void Player1SetsUpButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeValue(Player1SetsTextBox, 1);
    }

    private void Player1SetsDownButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeValue(Player1SetsTextBox, -1);
    }

    private void Player2SetsUpButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeValue(Player2SetsTextBox, 1);
    }

    private void Player2SetsDownButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeValue(Player2SetsTextBox, -1);
    }

    private void Player1LegsUpButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeValue(Player1LegsTextBox, 1);
    }

    private void Player1LegsDownButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeValue(Player1LegsTextBox, -1);
    }

    private void Player2LegsUpButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeValue(Player2LegsTextBox, 1);
    }

    private void Player2LegsDownButton_Click(object sender, RoutedEventArgs e)
    {
        ChangeValue(Player2LegsTextBox, -1);
    }

    private void ChangeValue(TextBox textBox, int delta)
    {
        if (int.TryParse(textBox.Text, out int currentValue))
        {
            int newValue = Math.Max(0, currentValue + delta);
            textBox.Text = newValue.ToString();
        }
        else
        {
            textBox.Text = "0";
        }
    }

    #endregion

    #region TextChanged Event Handlers
    
    private void Player1SetsTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ValidateNumericInput(sender as TextBox);
    }

    private void Player2SetsTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ValidateNumericInput(sender as TextBox);
    }

    private void Player1LegsTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ValidateNumericInput(sender as TextBox);
    }

    private void Player2LegsTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ValidateNumericInput(sender as TextBox);
    }

    private void ValidateNumericInput(TextBox? textBox)
    {
        if (textBox == null) return;

        // Allow only numeric input
        if (!int.TryParse(textBox.Text, out int value) || value < 0)
        {
            if (!string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = "0";
                textBox.SelectionStart = textBox.Text.Length; // Move cursor to end
            }
        }
    }

    #endregion

    private void ValidateAndUpdateWinner()
    {
        try
        {
            var p1Sets = _gameRules.PlayWithSets ? GetIntValue(Player1SetsTextBox.Text) : 0;
            var p2Sets = _gameRules.PlayWithSets ? GetIntValue(Player2SetsTextBox.Text) : 0;
            var p1Legs = GetIntValue(Player1LegsTextBox.Text);
            var p2Legs = GetIntValue(Player2LegsTextBox.Text);

            var validationResult = ValidateMatchResultStrict(p1Sets, p2Sets, p1Legs, p2Legs);
            
            if (validationResult.IsValid && validationResult.Winner != null)
            {
                HideValidationMessage();
                ShowWinner(validationResult.Winner, p1Sets, p2Sets, p1Legs, p2Legs);
                SaveButton.IsEnabled = true;
            }
            else
            {
                ShowValidationMessage(validationResult.ErrorMessage);
                HideWinner();
                SaveButton.IsEnabled = false;
            }
        }
        catch (Exception ex)
        {
            ShowValidationMessage(_localizationService.GetString("InvalidNumbers"));
            HideWinner();
            SaveButton.IsEnabled = false;
        }
    }

    private int GetIntValue(string text)
    {
        return int.TryParse(text, out int value) ? Math.Max(0, value) : 0;
    }

    private ValidationResult ValidateMatchResultStrict(int p1Sets, int p2Sets, int p1Legs, int p2Legs)
    {
        // Basis-Validierung: Negative Werte
        if (p1Sets < 0 || p2Sets < 0 || p1Legs < 0 || p2Legs < 0)
        {
            return new ValidationResult(false, _localizationService.GetString("NegativeValues"));
        }

        // VEREINFACHT: Sets werden nur validiert wenn PlayWithSets = true
        if (_gameRules.PlayWithSets)
        {
            System.Diagnostics.Debug.WriteLine($"MatchResultWindow.ValidateMatchResultStrict: Validating in SETS mode (SetsToWin={_gameRules.SetsToWin})");
            return ValidateSetsModeStrict(p1Sets, p2Sets, p1Legs, p2Legs);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"MatchResultWindow.ValidateMatchResultStrict: Validating in LEGS mode (LegsToWin={_gameRules.LegsToWin})");
            return ValidateLegsModeStrict(p1Legs, p2Legs);
        }
    }

    private ValidationResult ValidateSetsModeStrict(int p1Sets, int p2Sets, int p1Legs, int p2Legs)
    {
        var requiredSets = _gameRules.SetsToWin;
        var legsPerSet = _gameRules.LegsPerSet;
        var maxPossibleSets = (requiredSets * 2) - 1;

        // Validierung 1: Maximale Set-Anzahl
        if (p1Sets > requiredSets || p2Sets > requiredSets)
        {
            return new ValidationResult(false, _localizationService.GetString("InvalidSetCount", requiredSets, maxPossibleSets));
        }

        // Validierung 2: Unmögliche Set-Kombinationen
        if (p1Sets + p2Sets > maxPossibleSets)
        {
            return new ValidationResult(false, _localizationService.GetString("InvalidSetCount", maxPossibleSets, maxPossibleSets));
        }

        // Validierung 3: Beide Spieler können nicht gleichzeitig gewinnen
        if (p1Sets >= requiredSets && p2Sets >= requiredSets)
        {
            return new ValidationResult(false, _localizationService.GetString("BothPlayersWon"));
        }

        // Bestimme Gewinner
        Player? winner = null;
        if (p1Sets >= requiredSets) winner = _match.Player1;
        else if (p2Sets >= requiredSets) winner = _match.Player2;

        // Validierung 4: Leg-Konsistenz mit Sets
        var validationResult = ValidateLegsConsistencyWithSets(p1Sets, p2Sets, p1Legs, p2Legs, legsPerSet, winner);
        if (!validationResult.IsValid)
        {
            return validationResult;
        }

        // Validierung 5: Spiel muss beendet sein (ein Gewinner muss vorhanden sein)
        if (winner == null)
        {
            return new ValidationResult(false, _localizationService.GetString("MatchIncomplete"));
        }

        return new ValidationResult(true, null, winner);
    }

    private ValidationResult ValidateLegsConsistencyWithSets(int p1Sets, int p2Sets, int p1Legs, int p2Legs, int legsPerSet, Player? winner)
    {
        if (winner != null)
        {
            // Gewinner hat möglicherweise ein zusätzliches Set gewonnen
            int winnerSets = winner == _match.Player1 ? p1Sets : p2Sets;
            int loserSets = winner == _match.Player1 ? p2Sets : p1Sets;
            int winnerLegs = winner == _match.Player1 ? p1Legs : p2Legs;
            int loserLegs = winner == _match.Player1 ? p2Legs : p1Legs;
            
            // Validierung: Gewinner muss genügend Legs haben für seine gewonnenen Sets
            int minRequiredLegs = winnerSets * legsPerSet;
            if (winnerLegs < minRequiredLegs)
            {
                return new ValidationResult(false, _localizationService.GetString("InsufficientLegsForSet", 
                    winner.Name, minRequiredLegs));
            }
            
            // Validierung: Nicht zu viele Legs für die gewonnenen Sets
            int maxPossibleLegs = winnerSets * legsPerSet + (legsPerSet - 1); // Zusätzliche Legs im letzten Set
            if (winnerLegs > maxPossibleLegs)
            {
                return new ValidationResult(false, _localizationService.GetString("ExcessiveLegs", 
                    p1Sets, p2Sets, maxPossibleLegs));
            }

            // Validierung: Verlierer darf nicht zu viele Legs haben
            int maxLoserLegs = loserSets * legsPerSet + (legsPerSet - 1);
            if (loserLegs > maxLoserLegs)
            {
                return new ValidationResult(false, _localizationService.GetString("ExcessiveLegs", 
                    p1Sets, p2Sets, maxLoserLegs));
            }
        }
        else
        {
            // Kein Gewinner - prüfe ob jemand genügend Legs für einen Set-Sieg hat aber Sets nicht stimmen
            int p1ExpectedSets = p1Legs / legsPerSet;
            int p2ExpectedSets = p2Legs / legsPerSet;
            
            if (p1ExpectedSets > p1Sets)
            {
                return new ValidationResult(false, _localizationService.GetString("LegsExceedSetRequirement", 
                    _match.Player1?.Name ?? "Player 1"));
            }
            
            if (p2ExpectedSets > p2Sets)
            {
                return new ValidationResult(false, _localizationService.GetString("LegsExceedSetRequirement", 
                    _match.Player2?.Name ?? "Player 2"));
            }
        }

        return new ValidationResult(true);
    }

    private ValidationResult ValidateLegsModeStrict(int p1Legs, int p2Legs)
    {
        var requiredLegs = _gameRules.LegsToWin;
        var maxPossibleLegs = (requiredLegs * 2) - 1;

        // Validierung 1: Maximale Leg-Anzahl
        if (p1Legs > requiredLegs || p2Legs > requiredLegs)
        {
            return new ValidationResult(false, _localizationService.GetString("InvalidLegCount", requiredLegs, maxPossibleLegs));
        }

        // Validierung 2: Unmögliche Leg-Kombinationen
        if (p1Legs + p2Legs > maxPossibleLegs)
        {
            return new ValidationResult(false, _localizationService.GetString("InvalidLegCount", maxPossibleLegs, maxPossibleLegs));
        }

        // Validierung 3: Bestimme Gewinner
        bool p1HasWon = p1Legs >= requiredLegs && p1Legs > p2Legs;
        bool p2HasWon = p2Legs >= requiredLegs && p2Legs > p1Legs;
        
        if (p1HasWon && p2HasWon)
        {
            return new ValidationResult(false, _localizationService.GetString("BothPlayersWon"));
        }
        
        Player? winner = null;
        if (p1HasWon) winner = _match.Player1;
        else if (p2HasWon) winner = _match.Player2;
        
        // Validierung 4: Spiel muss beendet sein (ein Gewinner muss vorhanden sein)
        if (winner == null)
        {
            return new ValidationResult(false, _localizationService.GetString("MatchIncomplete"));
        }
        
        return new ValidationResult(true, null, winner);
    }

    private void ShowValidationMessage(string? message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            ValidationMessageBlock.Text = message;
            ValidationBorder.Visibility = Visibility.Visible;
        }
    }

    private void HideValidationMessage()
    {
        ValidationBorder.Visibility = Visibility.Collapsed;
    }

    private void ShowWinner(Player? winner, int p1Sets, int p2Sets, int p1Legs, int p2Legs)
    {
        if (winner != null)
        {
            var winnerText = $"{_localizationService.GetString("Winner")}: {winner.Name}";
            
            if (_gameRules.PlayWithSets)
            {
                winnerText += $" ({p1Sets}:{p2Sets} Sets, {p1Legs}:{p2Legs} Legs)";
                System.Diagnostics.Debug.WriteLine($"MatchResultWindow.ShowWinner: Sets mode - {winnerText}");
            }
            else
            {
                winnerText += $" ({p1Legs}:{p2Legs} Legs)";
                System.Diagnostics.Debug.WriteLine($"MatchResultWindow.ShowWinner: Legs mode - {winnerText}");
            }
            
            WinnerInfoBlock.Text = winnerText;
            WinnerBorder.Visibility = Visibility.Visible;
        }
    }

    private void HideWinner()
    {
        WinnerBorder.Visibility = Visibility.Collapsed;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // VEREINFACHT: Sets werden nur berücksichtigt wenn PlayWithSets = true
            var p1Sets = _gameRules.PlayWithSets ? GetIntValue(Player1SetsTextBox.Text) : 0;
            var p2Sets = _gameRules.PlayWithSets ? GetIntValue(Player2SetsTextBox.Text) : 0;
            var p1Legs = GetIntValue(Player1LegsTextBox.Text);
            var p2Legs = GetIntValue(Player2LegsTextBox.Text);

            System.Diagnostics.Debug.WriteLine($"MatchResultWindow.SaveButton_Click: PlayWithSets={_gameRules.PlayWithSets}");
            System.Diagnostics.Debug.WriteLine($"MatchResultWindow.SaveButton_Click: Saving p1Sets={p1Sets}, p2Sets={p2Sets}, p1Legs={p1Legs}, p2Legs={p2Legs}");

            // Finale strenge Validierung vor dem Speichern
            var validationResult = ValidateMatchResultStrict(p1Sets, p2Sets, p1Legs, p2Legs);
            
            if (!validationResult.IsValid)
            {
                // Zeige detaillierte Fehlermeldung und verhindere Speicherung
                MessageBox.Show(
                    $"{_localizationService.GetString("SaveBlocked")}\n\n{validationResult.ErrorMessage}", 
                    _localizationService.GetString("ValidationError"), 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return;
            }

            if (validationResult.Winner == null)
            {
                MessageBox.Show(
                    _localizationService.GetString("NoWinnerFound"), 
                    _localizationService.GetString("ValidationError"), 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                return;
            }

            // Alle Validierungen bestanden - Ergebnis speichern
            _match.SetResult(p1Sets, p2Sets, p1Legs, p2Legs, _gameRules.PlayWithSets);
            _match.Notes = NotesTextBox.Text;
            // WICHTIG: Setze UsesSets basierend auf den aktuell verwendeten GameRules
            _match.UsesSets = _gameRules.PlayWithSets;

            if (_match.Status == MatchStatus.NotStarted)
            {
                _match.StartTime = DateTime.Now;
            }

            System.Diagnostics.Debug.WriteLine($"MatchResultWindow.SaveButton_Click: Match.UsesSets set to {_match.UsesSets}");

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving result: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    /// <summary>
    /// Event-Handler für das Verschieben des Fensters über den Header
    /// </summary>
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    /// <summary>
    /// Gets the internal match object for result copying
    /// </summary>
    internal Match InternalMatch => _match;

    /// <summary>
    /// ✅ NEU: Initialize Hub Integration and QR Code
    /// UPDATED: Verwendet neue dart-scoring.html URL mit Match-UUID UND Tournament-ID Parameter
    /// FIXED: Verbesserte Debug-Ausgaben
    /// </summary>
    private void InitializeHubIntegration()
    {
     try
      {
  // ✅ IMPROVED DEBUG: Detaillierte Prüfung aller Bedingungen
  System.Diagnostics.Debug.WriteLine($"🎯 [MatchResultWindow] InitializeHubIntegration called");
   System.Diagnostics.Debug.WriteLine($"   HubService available: {_hubService != null}");
            System.Diagnostics.Debug.WriteLine($"   Hub registered: {_hubService?.IsRegisteredWithHub}");
   System.Diagnostics.Debug.WriteLine($"   Tournament ID: {_tournamentId ?? "null"}");  // ⭐ NEU
  System.Diagnostics.Debug.WriteLine($"   Match UUID: {_match.UniqueId ?? "null"}");
         
     // Prüfe ob Tournament beim Hub registriert ist und Match UUID vorhanden
    if (_hubService != null && 
     _hubService.IsRegisteredWithHub && 
  !string.IsNullOrEmpty(_match.UniqueId))
  {
       // ✅ FIXED: NEW URL FORMAT mit Tournament-ID UND Match-UUID
           if (!string.IsNullOrEmpty(_tournamentId))
    {
   // Mit Tournament-ID (empfohlen)
     _matchPageUrl = $"https://dtp.i3ull3t.de:9443/dart-scoring.html?tournament={_tournamentId}&match={_match.UniqueId}&uuid=true";
   System.Diagnostics.Debug.WriteLine($"🎯 [MatchResultWindow] ✅ Generated dart-scoring URL with Tournament ID: {_matchPageUrl}");
                }
     else
              {
   // Fallback: Ohne Tournament-ID (legacy)
         _matchPageUrl = $"https://dtp.i3ull3t.de:9443/dart-scoring.html?match={_match.UniqueId}&uuid=true";
         System.Diagnostics.Debug.WriteLine($"🎯 [MatchResultWindow] ⚠️ Generated dart-scoring URL WITHOUT Tournament ID (legacy): {_matchPageUrl}");
        }
        
        // Zeige QR Code Section
      QrCodeSection.Visibility = Visibility.Visible;
       
      // Generiere QR Code
       GenerateQrCode(_matchPageUrl);
        
                System.Diagnostics.Debug.WriteLine("✅ [MatchResultWindow] Hub integration initialized successfully");
            }
            else
 {
     // Verstecke QR Code Section
                QrCodeSection.Visibility = Visibility.Collapsed;
  
    var reasons = new List<string>();
          if (_hubService == null) reasons.Add("HubService not available");
       if (_hubService?.IsRegisteredWithHub != true) reasons.Add("Tournament not registered with hub");
           if (string.IsNullOrEmpty(_match.UniqueId)) reasons.Add("Match UUID not available");
     
  System.Diagnostics.Debug.WriteLine($"ℹ️ [MatchResultWindow] QR Code hidden - Reasons: {string.Join(", ", reasons)}");
          }
        }
        catch (Exception ex)
        {
 System.Diagnostics.Debug.WriteLine($"❌ [MatchResultWindow] Error initializing hub integration: {ex.Message}");
     QrCodeSection.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// ✅ NEU: Generate QR Code for Match-Page URL
    /// </summary>
    private void GenerateQrCode(string url)
    {
        try
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M);
                
                using (var qrCode = new QRCoder.QRCode(qrCodeData))
                {
                    // Generiere QR Code als Bitmap
                    using (var qrCodeBitmap = qrCode.GetGraphic(20, Color.Black, Color.White, true))
                    {
                        // Konvertiere zu WPF BitmapImage
                        var bitmapImage = new BitmapImage();
                        using (var stream = new MemoryStream())
                        {
                            qrCodeBitmap.Save(stream, ImageFormat.Png);
                            stream.Position = 0;
                            
                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = stream;
                            bitmapImage.EndInit();
                            bitmapImage.Freeze(); // Wichtig für Thread-Safety
                        }
                        
                        // Setze QR Code Image
                        QrCodeImage.Source = bitmapImage;
                        
                        System.Diagnostics.Debug.WriteLine("✅ [MatchResultWindow] QR Code generated successfully");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [MatchResultWindow] Error generating QR code: {ex.Message}");
            
            // Fallback: Zeige Fehler-Icon
            try
            {
                // Erstelle einfaches Fehler-Bitmap
                using (var errorBitmap = new Bitmap(100, 100))
                {
                    using (var g = Graphics.FromImage(errorBitmap))
                    {
                        g.Clear(Color.LightGray);
                        g.DrawString("❌", new System.Drawing.Font("Arial", 32), Brushes.Red, 25, 25);
                    }
                    
                    var bitmapImage = new BitmapImage();
                    using (var stream = new MemoryStream())
                    {
                        errorBitmap.Save(stream, ImageFormat.Png);
                        stream.Position = 0;
                        
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = stream;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                    }
                    
                    QrCodeImage.Source = bitmapImage;
                }
            }
            catch
            {
                // Wenn auch das fehlschlägt, verstecke die Section
                QrCodeSection.Visibility = Visibility.Collapsed;
            }
        }
    }

    /// <summary>
    /// ✅ NEU: Open Match-Page in default browser
    /// UPDATED: Öffnet dart-scoring.html mit Match-UUID Parameter
    /// </summary>
    private void OpenWebInterfaceButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(_matchPageUrl))
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = _matchPageUrl,
                    UseShellExecute = true
                };
                
                Process.Start(processInfo);
                
                System.Diagnostics.Debug.WriteLine($"🌐 [MatchResultWindow] Opened dart-scoring page in browser: {_matchPageUrl}");
                
                // Optional: Zeige Toast-Nachricht
                MessageBox.Show(
                    "Dart-Scoring Seite wurde im Browser geöffnet!\n\nSie können das Ergebnis nun auch über das Web-Interface eingeben.",
                    "Web-Interface geöffnet",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Fehler beim Öffnen der Dart-Scoring Seite:\n{ex.Message}",
                "Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
                
            System.Diagnostics.Debug.WriteLine($"❌ [MatchResultWindow] Error opening browser: {ex.Message}");
        }
    }

    /// <summary>
    /// ✅ NEU: Copy Match-Page URL to clipboard
    /// UPDATED: Kopiert dart-scoring.html URL mit Match-UUID Parameter
    /// </summary>
    private void CopyUrlButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(_matchPageUrl))
            {
                Clipboard.SetText(_matchPageUrl);
                
                MessageBox.Show(
                    "Dart-Scoring URL wurde in die Zwischenablage kopiert!\n\n" + 
                    "Sie können diese URL teilen, um anderen den Zugang zur Match-Eingabe zu ermöglichen.",
                    "URL kopiert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                
                System.Diagnostics.Debug.WriteLine($"📋 [MatchResultWindow] URL copied to clipboard: {_matchPageUrl}");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Fehler beim Kopieren der URL:\n{ex.Message}",
                "Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
                
            System.Diagnostics.Debug.WriteLine($"❌ [MatchResultWindow] Error copying URL: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private class ValidationResult
    {
        public bool IsValid { get; }
        public string? ErrorMessage { get; }
        public Player? Winner { get; }

        public ValidationResult(bool isValid, string? errorMessage = null, Player? winner = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
            Winner = winner;
        }
    }
}