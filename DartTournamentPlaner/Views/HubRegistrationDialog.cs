using System;
using System.Windows;
using System.Windows.Documents;  // ⭐ NEU: Für Run-Elemente
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views;

/// <summary>
/// Dialog für Hub-Registrierung mit optionaler custom Tournament-ID
/// </summary>
public partial class HubRegistrationDialog : Window
{
    private readonly LocalizationService? _localizationService;
    private readonly LicensedHubService _hubService;
    private readonly TournamentManagementService _tournamentService;
    private readonly Action _markAsChanged;

    public bool RegistrationSuccessful { get; private set; }

  public HubRegistrationDialog(
        LicensedHubService hubService,
    TournamentManagementService tournamentService,
      Action markAsChanged,
  LocalizationService? localizationService = null)
  {
      InitializeComponent();
    
     _hubService = hubService ?? throw new ArgumentNullException(nameof(hubService));
  _tournamentService = tournamentService ?? throw new ArgumentNullException(nameof(tournamentService));
     _markAsChanged = markAsChanged ?? throw new ArgumentNullException(nameof(markAsChanged));
        _localizationService = localizationService;
        
  // ⭐ NEU: Übersetze UI-Elemente
    ApplyTranslations();

  // Lade gespeicherte Tournament-ID falls vorhanden
        LoadSavedTournamentId();
        
        // Setze Focus auf TextBox
  Loaded += (s, e) => TournamentIdTextBox.Focus();
 }

/// <summary>
    /// ⭐ NEU: Wendet Übersetzungen auf UI-Elemente an
    /// </summary>
    private void ApplyTranslations()
    {
        if (_localizationService == null) return;

        try
{
   // Window
            Title = _localizationService.GetString("HubRegistrationDialogTitle") ?? "Mit Hub registrieren";
         
     // Header
      if (FindName("HeaderTitle") is System.Windows.Controls.TextBlock headerTitle)
      {
     headerTitle.Text = "🔗 " + (_localizationService.GetString("HubRegistrationDialogTitle") ?? "Mit Hub registrieren");
      }
            
if (FindName("HeaderSubtitle") is System.Windows.Controls.TextBlock headerSubtitle)
   {
 headerSubtitle.Text = _localizationService.GetString("HubRegistrationDialogSubtitle") ?? "Registrieren Sie Ihr Turnier für Live-Scoring";
       }
          
      // Labels
if (FindName("TournamentIdLabel") is System.Windows.Controls.TextBlock idLabel)
  {
     idLabel.Text = _localizationService.GetString("TournamentIdLabel") ?? "Tournament-ID (optional):";
    }
            
    // Tooltips
  TournamentIdTextBox.ToolTip = _localizationService.GetString("TournamentIdPlaceholder") ?? "Leer lassen für automatische ID-Generierung";
  GenerateIdButton.ToolTip = _localizationService.GetString("GenerateNewIdTooltip") ?? "Neue ID generieren";
   
    // Info-Text TextBlocks
   if (FindName("TipTextBlock") is System.Windows.Controls.TextBlock tipTextBlock)
      {
   var tipTitle = _localizationService.GetString("TournamentIdTipTitle") ?? "💡 Tipp: ";
        var tipText = _localizationService.GetString("TournamentIdTipText") ?? "Lassen Sie das Feld leer für automatische ID-Generierung.";
 tipTextBlock.Text = tipTitle + tipText;
   }

  if (FindName("NoteTextBlock") is System.Windows.Controls.TextBlock noteTextBlock)
        {
  var noteTitle = _localizationService.GetString("TournamentIdNoteTitle") ?? "📌 Hinweis: ";
    var noteText = _localizationService.GetString("TournamentIdNoteText") ?? "Verwenden Sie eine bekannte ID um ein bestehendes Turnier fortzusetzen.";
        noteTextBlock.Text = noteTitle + noteText;
 }
 
// Buttons
   CancelButton.Content = _localizationService.GetString("CancelButton") ?? "Abbrechen";
      RegisterButton.Content = _localizationService.GetString("RegisterButton") ?? "Registrieren";
     }
    catch (Exception ex)
  {
   System.Diagnostics.Debug.WriteLine($"⚠️ Error applying translations: {ex.Message}");
   }
 }

    /// <summary>
    /// Lädt gespeicherte Tournament-ID falls vorhanden
    /// </summary>
    private void LoadSavedTournamentId()
    {
        try
        {
    var tournamentData = _tournamentService.GetTournamentData();
      if (tournamentData != null && !string.IsNullOrWhiteSpace(tournamentData.TournamentId))
     {
         TournamentIdTextBox.Text = tournamentData.TournamentId;
      System.Diagnostics.Debug.WriteLine($"📂 Loaded saved Tournament ID: {tournamentData.TournamentId}");
    }
        }
        catch (Exception ex)
   {
     System.Diagnostics.Debug.WriteLine($"⚠️ Could not load saved Tournament ID: {ex.Message}");
     }
    }

    /// <summary>
    /// Generiert eine neue Tournament-ID
    /// </summary>
    private void GenerateNewId_Click(object sender, RoutedEventArgs e)
    {
        try
  {
            var newId = _hubService.InnerHubService.GenerateNewTournamentId();
       TournamentIdTextBox.Text = newId;
       
       System.Diagnostics.Debug.WriteLine($"🔄 Generated new Tournament ID: {newId}");
     }
 catch (Exception ex)
        {
          System.Diagnostics.Debug.WriteLine($"❌ Error generating new ID: {ex.Message}");
        
    var errorTitle = _localizationService?.GetString("Error") ?? "Fehler";
    var errorMessage = $"{_localizationService?.GetString("CopyError") ?? "Fehler"}: {ex.Message}";
            
     MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    }

    /// <summary>
    /// Registriert Tournament mit Hub
    /// </summary>
    private async void Register_Click(object sender, RoutedEventArgs e)
    {
        try
        {
          // Deaktiviere Buttons während Registrierung
   RegisterButton.IsEnabled = false;
     CancelButton.IsEnabled = false;
        GenerateIdButton.IsEnabled = false;
       TournamentIdTextBox.IsEnabled = false;
            
       // ⭐ NEU: Zeige "Registriere..." Text
            var originalContent = RegisterButton.Content;
  RegisterButton.Content = _localizationService?.GetString("RegisteringTournament") ?? "Registriere...";

            // Hole custom ID (oder null für auto-generation)
      string? customTournamentId = null;
       if (!string.IsNullOrWhiteSpace(TournamentIdTextBox.Text))
  {
  customTournamentId = TournamentIdTextBox.Text.Trim();
 System.Diagnostics.Debug.WriteLine($"🎯 {_localizationService?.GetString("CustomTournamentIdUsed") ?? "User provided custom Tournament ID"}: {customTournamentId}");
     }
 else
  {
         System.Diagnostics.Debug.WriteLine($"🎯 {_localizationService?.GetString("AutoGeneratedTournamentId") ?? "No custom ID provided, will auto-generate"}");
     }

      // Registriere mit Hub
     var success = await _hubService.RegisterTournamentAsync(customTournamentId);

          if (success)
       {
  // Speichere Tournament-ID in TournamentData
     var tournamentId = _hubService.GetCurrentTournamentId();
    var tournamentData = _tournamentService.GetTournamentData();

       if (tournamentData != null && !string.IsNullOrEmpty(tournamentId))
  {
      tournamentData.TournamentId = tournamentId;
  System.Diagnostics.Debug.WriteLine($"✅ [HubRegistrationDialog] Tournament-ID saved to TournamentData: {tournamentId}");

         // Speichere die Änderung
  _markAsChanged();
     }

    // Sync Tournament Data
    await _hubService.SyncTournamentAsync(_tournamentService.GetTournamentData());

     RegistrationSuccessful = true;
    DialogResult = true;
   Close();
  }
   else
          {
     // Fehler wird bereits durch LicensedHubService Dialog angezeigt
  // Re-enable Buttons für Retry
          RegisterButton.Content = originalContent;
     RegisterButton.IsEnabled = true;
       CancelButton.IsEnabled = true;
  GenerateIdButton.IsEnabled = true;
          TournamentIdTextBox.IsEnabled = true;
  }
        }
        catch (Exception ex)
        {
      System.Diagnostics.Debug.WriteLine($"❌ Error during registration: {ex.Message}");

      var errorTitle = _localizationService?.GetString("Error") ?? "Fehler";
      var errorMessage = $"{_localizationService?.GetString("RegisterTournamentError") ?? "Fehler bei der Registrierung"}: {ex.Message}";
      
            MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);

      // Re-enable Buttons
RegisterButton.IsEnabled = true;
   CancelButton.IsEnabled = true;
 GenerateIdButton.IsEnabled = true;
    TournamentIdTextBox.IsEnabled = true;
        }
    }

    /// <summary>
    /// Abbrechen
    /// </summary>
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
    DialogResult = false;
        Close();
    }

    /// <summary>
    /// Statische Methode zum Öffnen des Dialogs
    /// </summary>
    public static bool ShowDialog(
        Window? owner,
      LicensedHubService hubService,
        TournamentManagementService tournamentService,
        Action markAsChanged,
      LocalizationService? localizationService = null)
    {
   var dialog = new HubRegistrationDialog(hubService, tournamentService, markAsChanged, localizationService);

        if (owner != null)
        {
            dialog.Owner = owner;
     }

return dialog.ShowDialog() == true && dialog.RegistrationSuccessful;
    }
}
