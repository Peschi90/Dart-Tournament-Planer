using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.License;

namespace DartTournamentPlaner.Views.License;

/// <summary>
/// Moderner Dialog für Lizenzkauf-Anfragen im Anwendungsdesign
/// </summary>
public partial class PurchaseLicenseDialog : Window
{
    private readonly LocalizationService _localizationService;
    private readonly LicenseManager _licenseManager;

    public PurchaseLicenseDialog(LocalizationService localizationService, LicenseManager licenseManager)
    {
        _localizationService = localizationService;
        _licenseManager = licenseManager;
        
        InitializeComponent();
        InitializeWindow();
        LoadSystemInformation();
    }

    private void InitializeWindow()
    {
        Title = _localizationService.GetString("PurchaseLicense") ?? "Lizenz kaufen";
        
        // Übersetzungen anwenden
        TitleText.Text = _localizationService.GetString("PurchaseLicenseTitle") ?? "Lizenz kaufen";
        SubtitleText.Text = _localizationService.GetString("PurchaseLicenseSubtitle") ?? 
            "Füllen Sie das Formular aus, um eine Lizenz für Dart Tournament Planner anzufordern";
        PurchasePersonalInformation.Text = _localizationService.GetString("PurchasePersonalInformation") ?? "Perfönliche Informationen";
        PurchaseLicenseRequirements.Text = _localizationService.GetString("PurchaseLicenseRequirements") ?? "Lizenz Vorraussetzungen";
        PurchaseAdditionalInformation.Text = _localizationService.GetString("PurchaseAdditionalInformation") ?? "Zusätzliche Informationen";
        PurchaseSystemInformation.Text = _localizationService.GetString("PurchaseSystemInformation") ?? "System Informationen";
        PurchaseHardwareInfoI.Text = _localizationService.GetString("PurchaseHardwareInfoI") ?? "InfoText1";
        PurchaseHardwareInfoII.Text = _localizationService.GetString("PurchaseHardwareInfoII") ?? "InfoText2";

        // Labels
        FirstNameLabel.Text = _localizationService.GetString("FirstName") ?? "Vorname" + " *";
        LastNameLabel.Text = _localizationService.GetString("LastName") ?? "Nachname" + " *";
        EmailLabel.Text = _localizationService.GetString("Email") ?? "E-Mail-Adresse" + " *";
        CompanyLabel.Text = _localizationService.GetString("Company") ?? "Unternehmen / Organisation";
        
        LicenseTypeLabel.Text = _localizationService.GetString("LicenseType") ?? "Lizenztyp" + " *";
        //ActivationsLabel.Text = _localizationService.GetString("RequiredActivations") ?? "Anzahl benötigter Aktivierungen";
        FeaturesLabel.Text = _localizationService.GetString("RequiredFeatures") ?? "Benötigte Features";
        
        MessageLabel.Text = _localizationService.GetString("AdditionalMessage") ?? "Nachricht / Besondere Anforderungen";
        SourceLabel.Text = _localizationService.GetString("HowDidYouHear") ?? "Wie haben Sie von uns erfahren?";
        
        // Buttons
        CancelButton.Content = _localizationService.GetString("Cancel") ?? "Abbrechen";
        SendRequestButton.Content = _localizationService.GetString("SendLicenseRequest") ?? "Lizenzanfrage senden";
        
        // Standardwerte setzen
        LicenseTypeComboBox.SelectedIndex = 0; // Personal License
        //ActivationsComboBox.SelectedIndex = 0; // 3 Aktivierungen
        
        // Häufig verwendete Features vorauswählen
        StatisticsFeature.IsChecked = true;
        HubConnectionFeature.IsChecked = true;
        ApiConnectionFeature.IsChecked = true;
        PrintFeature.IsChecked = true;  // Sicherstellen dass Print Feature ausgewählt ist
    }

    /// <summary>
    /// Setzt den Fokus auf das Print Feature (z.B. wenn Dialog aus Print-Lizenz-Anfrage geöffnet wird)
    /// </summary>
    public void FocusOnPrintFeature()
    {
        try
        {
            PrintFeature.IsChecked = true;
            PrintFeature.Focus();
            
            // Scroll zu dem Feature falls nötig
            PrintFeature.BringIntoView();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error focusing on print feature: {ex.Message}");
        }
    }

    /// <summary>
    /// Setzt den Fokus auf das Statistics Feature (z.B. wenn Dialog aus Statistics-Lizenz-Anfrage geöffnet wird)
    /// </summary>
    public void FocusOnStatisticsFeature()
    {
        try
        {
            StatisticsFeature.IsChecked = true;
            HubConnectionFeature.IsChecked = true; // Statistics benötigt Hub-Verbindung
            StatisticsFeature.Focus();
            
            // Scroll zu dem Feature falls nötig
            StatisticsFeature.BringIntoView();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error focusing on statistics feature: {ex.Message}");
        }
    }

    /// <summary>
    /// Setzt den Fokus auf das Hub Connection Feature (z.B. wenn Dialog aus Hub-Lizenz-Anfrage geöffnet wird)
    /// </summary>
    public void FocusOnHubFeature()
    {
        try
        {
            HubConnectionFeature.IsChecked = true;
            HubConnectionFeature.Focus();
            
            // Scroll zu dem Feature falls nötig
            HubConnectionFeature.BringIntoView();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error focusing on hub feature: {ex.Message}");
        }
    }

    /// <summary>
    /// Setzt den Fokus auf das Tournament Overview Feature (z.B. wenn Dialog aus Tournament Overview-Lizenz-Anfrage geöffnet wird)
    /// </summary>
    public void FocusOnTournamentOverviewFeature()
    {
        try
        {
            TournamentOverviewFeature.IsChecked = true;
            TournamentOverviewFeature.Focus();
            
            // Scroll zu dem Feature falls nötig
            TournamentOverviewFeature.BringIntoView();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error focusing on tournament overview feature: {ex.Message}");
        }
    }

    private void LoadSystemInformation()
    {
        try
        {
            HardwareIdText.Text = LicenseManager.GenerateHardwareId();
        }
        catch (Exception ex)
        {
            HardwareIdText.Text = $"Error generating Hardware ID: {ex.Message}";
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void SendRequestButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Validierung
            if (!ValidateForm())
                return;

            // E-Mail-Inhalt erstellen
            var emailContent = BuildEmailContent();
            
            // E-Mail öffnen
            var success = OpenEmailClient(emailContent);
            
            if (success)
            {
                var title = _localizationService.GetString("Success") ?? "Erfolg";
                var message = _localizationService.GetString("EmailClientOpened") ?? 
                    "Ihr E-Mail-Client wurde geöffnet mit einer vorbereiteten Lizenzanfrage.\n\n" +
                    "Bitte senden Sie die E-Mail ab, um Ihre Lizenzanfrage zu übermitteln.\n" +
                    "Sie erhalten innerhalb von 24 Stunden eine Antwort.";
                
                //MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Zeige Spenden-Dialog nach erfolgreicher E-Mail-Erstellung
                ShowDonationDialog();
                
                DialogResult = true;
                Close();
            }
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Öffnen des E-Mail-Clients:\n\n{ex.Message}\n\n" +
                         "Bitte wenden Sie sich direkt an support@license-dtp.i3ull3t.de";
            
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool ValidateForm()
    {
        var errors = new StringBuilder();

        if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            errors.AppendLine("• Vorname ist erforderlich");
        
        if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            errors.AppendLine("• Nachname ist erforderlich");
        
        if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            errors.AppendLine("• E-Mail-Adresse ist erforderlich");
        else if (!IsValidEmail(EmailTextBox.Text))
            errors.AppendLine("• Bitte geben Sie eine gültige E-Mail-Adresse ein");
        
        if (LicenseTypeComboBox.SelectedItem == null)
            errors.AppendLine("• Bitte wählen Sie einen Lizenztyp aus");

        if (errors.Length > 0)
        {
            var title = _localizationService.GetString("ValidationError") ?? "Validierungsfehler";
            var message = "Bitte korrigieren Sie die folgenden Fehler:\n\n" + errors.ToString();
            
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private string BuildEmailContent()
    {
        var content = new StringBuilder();
        
        // Header
        content.AppendLine("Subject: License Request - Dart Tournament Planner");
        content.AppendLine();
        content.AppendLine("Hello Dart Tournament Planner Support Team,");
        content.AppendLine();
        content.AppendLine("I would like to request a license for Dart Tournament Planner with the following details:");
        content.AppendLine();
        
        // Personal Information
        content.AppendLine("=== PERSONAL INFORMATION ===");
        content.AppendLine($"Name: {FirstNameTextBox.Text} {LastNameTextBox.Text}");
        content.AppendLine($"Email: {EmailTextBox.Text}");
        
        if (!string.IsNullOrWhiteSpace(CompanyTextBox.Text))
            content.AppendLine($"Company: {CompanyTextBox.Text}");
        content.AppendLine();
        
        // License Requirements
        content.AppendLine("=== LICENSE REQUIREMENTS ===");
        
        var selectedLicenseType = LicenseTypeComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem;
        if (selectedLicenseType != null)
            content.AppendLine($"License Type: {selectedLicenseType.Content}");
        
        //var selectedActivations = ActivationsComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem;
        //if (selectedActivations != null)
        //    content.AppendLine($"Required Activations: {selectedActivations.Content}");
        
        // Features
        var selectedFeatures = GetSelectedFeatures();
        if (selectedFeatures.Any())
        {
            content.AppendLine();
            content.AppendLine("Required Features:");
            foreach (var feature in selectedFeatures)
            {
                content.AppendLine($"• {feature}");
            }
        }
        content.AppendLine();
        
        // System Information
        content.AppendLine("=== SYSTEM INFORMATION ===");
        content.AppendLine($"Hardware ID: {HardwareIdText.Text}");
        content.AppendLine($"Application Version: {_localizationService.GetApplicationVersion()}");
        content.AppendLine($"Operating System: {Environment.OSVersion}");
        content.AppendLine();
        
        // Additional Information
        if (!string.IsNullOrWhiteSpace(MessageTextBox.Text))
        {
            content.AppendLine("=== ADDITIONAL INFORMATION ===");
            content.AppendLine(MessageTextBox.Text);
            content.AppendLine();
        }
        
        var selectedSource = SourceComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem;
        if (selectedSource != null)
        {
            content.AppendLine($"How did you hear about us: {selectedSource.Content}");
            content.AppendLine();
        }
        
        // Footer
        content.AppendLine("Please provide me with:");
        content.AppendLine("• License pricing information");
        content.AppendLine("• Payment methods available");
        content.AppendLine("• Estimated delivery time");
        content.AppendLine();
        content.AppendLine("Thank you for your time and support!");
        content.AppendLine();
        content.AppendLine("Best regards,");
        content.AppendLine($"{FirstNameTextBox.Text} {LastNameTextBox.Text}");
        
        return content.ToString();
    }

    private string[] GetSelectedFeatures()
    {
        var features = new List<string>();
        
        if (StatisticsFeature.IsChecked == true)
            features.Add("Advanced Statistics");
        if (HubConnectionFeature.IsChecked == true)
            features.Add("Tournament Hub Connection");
        if (ApiConnectionFeature.IsChecked == true)
            features.Add("API Access");
        if (PrintFeature.IsChecked == true)
            features.Add("Enhanced Printing");
        if (TournamentOverviewFeature.IsChecked == true)
            features.Add("Tournament Overview");

        
        return features.ToArray();
    }

    private bool OpenEmailClient(string emailContent)
    {
        try
        {
            var recipient = "support@license-dtp.i3ull3t.de";
            var subject = "License Request - Dart Tournament Planner";
            
            // Split content to extract subject and body properly
            var lines = emailContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var bodyLines = new List<string>();
            bool skipFirstLines = true;
            
            foreach (var line in lines)
            {
                if (skipFirstLines && (line.StartsWith("Subject:") || string.IsNullOrEmpty(line)))
                {
                    if (line.StartsWith("Subject:"))
                        skipFirstLines = false;
                    continue;
                }
                bodyLines.Add(line);
            }
            
            var body = string.Join("\n", bodyLines);
            
            // Mailto-URL erstellen
            var mailtoUrl = $"mailto:{recipient}?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";
            
            // Standard-E-Mail-Client öffnen
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = mailtoUrl,
                UseShellExecute = true
            };
            
            System.Diagnostics.Process.Start(processInfo);
            return true;
        }
        catch (Exception ex)
        {
            // Fallback: Zeige E-Mail-Inhalt in MessageBox
            var title = "E-Mail-Inhalt kopieren";
            var message = "E-Mail-Client konnte nicht geöffnet werden. Hier ist der E-Mail-Inhalt zum manuellen Kopieren:\n\n" +
                         "Empfänger: support@license-dtp.i3ull3t.de\n\n" +
                         emailContent + "\n\n" +
                         $"Fehler: {ex.Message}";
            
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            
            // Inhalt in Zwischenablage kopieren
            try
            {
                Clipboard.SetText($"To: support@license-dtp.i3ull3t.de\n\n{emailContent}");
            }
            catch { }
            
            return true;
        }
    }

    private void ShowDonationDialog()
    {
        try
        {
            // Verwende den statischen Helper um den Dialog anzuzeigen
            var donationAmount = DonationSelectionDialog.ShowDonationDialog(this, _localizationService);
            
            if (donationAmount.HasValue && donationAmount.Value > 0)
            {
                // Benutzer hat gespendet - hier könnte man zusätzliche Aktionen durchführen
                System.Diagnostics.Debug.WriteLine($"User donated: {donationAmount.Value:F2} EUR");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing donation dialog: {ex.Message}");
            // Fehler beim Spenden-Dialog sind nicht kritisch - einfach weiter
        }
    }
}