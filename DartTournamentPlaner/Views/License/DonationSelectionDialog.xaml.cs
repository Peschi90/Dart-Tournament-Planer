using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views.License;

/// <summary>
/// Moderner Dialog für Spenden-Auswahl nach Lizenzanfrage
/// </summary>
public partial class DonationSelectionDialog : Window
{
    private readonly LocalizationService _localizationService;
    private decimal _selectedAmount = 5.0m; // Standard: 5€

    public DonationSelectionDialog(LocalizationService localizationService)
    {
        _localizationService = localizationService;
        
        InitializeComponent();
        InitializeWindow();
    }

    private void InitializeWindow()
    {
        Title = _localizationService.GetString("SupportProject") ?? "Projekt unterstützen";
        
        // Übersetzungen anwenden
        TitleText.Text = _localizationService.GetString("SupportProjectTitle") ?? "Unser Projekt unterstützen";
        SubtitleText.Text = _localizationService.GetString("SupportProjectSubtitle") ?? 
            "Ihre Spende hilft uns, Dart Tournament Planner weiterzuentwickeln!";

        SelectAmountLabel.Text = _localizationService.GetString("SelectDonationAmount") ?? "Wählen Sie Ihren Spendenbetrag:";
        
        // Button-Texte
        SkipButton.Content = _localizationService.GetString("Skip") ?? "Überspringen";
        DonateButton.Content = "💝 " + (_localizationService.GetString("DonateViaPayPal") ?? "Via PayPal spenden");
        
        // Info-Text lokalisieren
        UpdateInfoText();
        
        // Event-Handler für RadioButton-Änderungen
        Donation1Euro.Checked += (s, e) => _selectedAmount = 1.0m;
        Donation2Euro.Checked += (s, e) => _selectedAmount = 2.0m;
        Donation5Euro.Checked += (s, e) => _selectedAmount = 5.0m;
        Donation10Euro.Checked += (s, e) => _selectedAmount = 10.0m;
        DonationNone.Checked += (s, e) => _selectedAmount = 0.0m;
    }

    private void UpdateInfoText()
    {
        var germanText = @"- Hält die Software kostenlos und Open Source
- Unterstützt laufende Entwicklung und neue Features  
- Hilft bei Server- und Infrastruktur-Kosten
- Zeigt Wertschätzung für unsere Arbeit";

        var englishText = @"- Keeps the software free and open source
- Supports ongoing development and new features
- Helps maintain servers and infrastructure  
- Shows appreciation for our work";

        InfoText.Text = _localizationService.CurrentLanguage == "de" ? germanText : englishText;
    }

    //private void CustomAmountTextBox_GotFocus(object sender, RoutedEventArgs e)
    //{
    //    // Automatisch Custom-Option auswählen wenn in TextBox geklickt wird
    //    DonationCustom.IsChecked = true;
    //    UpdateCustomAmount();
        
    //    // Text auswählen für einfache Eingabe
    //    if (sender is TextBox textBox)
    //    {
    //        textBox.SelectAll();
    //    }
    //}

    //private void CustomAmountTextBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    //{
    //    // Automatisch Custom-Option auswählen und Event nicht weiterleiten
    //    DonationCustom.IsChecked = true;
    //    UpdateCustomAmount();
        
    //    // Fokus auf TextBox setzen
    //    if (sender is TextBox textBox)
    //    {
    //        textBox.Focus();
    //        textBox.SelectAll();
    //    }
        
    //    // Event als behandelt markieren, damit RadioButton nicht das Ereignis abfängt
    //    e.Handled = true;
    //}

    //private void DonationCustom_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    //{
    //    // Wenn auf das RadioButton aber nicht auf die TextBox geklickt wird
    //    if (e.OriginalSource is TextBox)
    //    {
    //        // TextBox-Event hat Priorität
    //        return;
    //    }
        
    //    // Fokus auf TextBox setzen wenn RadioButton angeklickt wird
    //    CustomAmountTextBox.Focus();
    //    CustomAmountTextBox.SelectAll();
    //}

    //private void CustomAmountTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    //{
    //    // Nur Zahlen, Komma, Punkt, Backspace, Delete, Tab und Pfeiltasten erlauben
    //    if (e.Key == System.Windows.Input.Key.Back || 
    //        e.Key == System.Windows.Input.Key.Delete || 
    //        e.Key == System.Windows.Input.Key.Tab ||
    //        e.Key == System.Windows.Input.Key.Left ||
    //        e.Key == System.Windows.Input.Key.Right ||
    //        e.Key == System.Windows.Input.Key.Home ||
    //        e.Key == System.Windows.Input.Key.End)
    //    {
    //        return; // Diese Tasten sind erlaubt
    //    }
        
    //    // Zahlen erlauben
    //    if (e.Key >= System.Windows.Input.Key.D0 && e.Key <= System.Windows.Input.Key.D9)
    //    {
    //        return;
    //    }
        
    //    // Numpad-Zahlen erlauben
    //    if (e.Key >= System.Windows.Input.Key.NumPad0 && e.Key <= System.Windows.Input.Key.NumPad9)
    //    {
    //        return;
    //    }
        
    //    // Komma und Punkt für Dezimalzahlen (aber nur eins)
    //    var textBox = sender as TextBox;
    //    if ((e.Key == System.Windows.Input.Key.OemComma || 
    //         e.Key == System.Windows.Input.Key.Decimal || 
    //         e.Key == System.Windows.Input.Key.OemPeriod) &&
    //        textBox != null &&
    //        !textBox.Text.Contains(",") && 
    //        !textBox.Text.Contains(".") && textBox.Text.Length > 0)
    //    {
    //        return;
    //    }
        
    //    // Enter zum Bestätigen
    //    if (e.Key == System.Windows.Input.Key.Enter)
    //    {
    //        DonateButton.Focus(); // Fokus auf Donate Button
    //        return;
    //    }
        
    //    // Alle anderen Tasten blockieren
    //    e.Handled = true;
    //}

    //private void CustomAmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
    //{
    //    if (DonationCustom.IsChecked == true)
    //    {
    //        UpdateCustomAmount();
    //    }
    //}

    //private void UpdateCustomAmount()
    //{
    //    var text = CustomAmountTextBox.Text.Replace(",", "."); // Deutsche Kommata zu Punkten
        
    //    if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
    //    {
    //        _selectedAmount = Math.Max(0, Math.Min(amount, 9999)); // Limitiere auf sinnvolle Beträge
    //    }
    //    else
    //    {
    //        _selectedAmount = 10.0m; // Fallback
    //    }
    //}

    private void SkipButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void DonateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_selectedAmount <= 0)
            {
                // Benutzer hat "Ohne Spende" gewählt
                DialogResult = false;
                Close();
                return;
            }

            // PayPal.me URL erstellen
            var paypalUrl = BuildPayPalMeUrl(_selectedAmount);
            
            // PayPal Link öffnen
            var success = OpenPayPalLink(paypalUrl);
            
            if (success)
            {
                var title = _localizationService.GetString("ThankYou") ?? "Vielen Dank!";
                var message = _localizationService.GetString("PayPalOpened") ?? 
                    $"PayPal wurde geöffnet für eine Spende von {_selectedAmount:F2} EUR.\n\n" +
                    "Vielen Dank für Ihre Unterstützung!\n\n" +
                    "Ihre Spende hilft uns dabei, Dart Tournament Planner kostenlos und " +
                    "mit neuen Features weiterzuentwickeln.";
                
                //MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                
                DialogResult = true;
                Close();
            }
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Öffnen von PayPal:\n\n{ex.Message}\n\n" +
                         "Sie können auch direkt spenden unter:\nhttps://paypal.me/darttournamentplanner";
            
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static string BuildPayPalMeUrl(decimal amount)
    {
        // PayPal.me Format: https://paypal.me/USERNAME/AMOUNT
        // Betrag mit 2 Dezimalstellen und Punkt als Dezimaltrennzeichen
        var formattedAmount = amount.ToString("F2", CultureInfo.InvariantCulture);
        
        // TODO: Hier den echten PayPal.me Username eintragen
        var paypalUsername = "i3ull3t"; // Beispiel - durch echten Username ersetzen
        
        return $"https://paypal.me/{paypalUsername}/{formattedAmount}EUR";
    }

    private static bool OpenPayPalLink(string paypalUrl)
    {
        try
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = paypalUrl,
                UseShellExecute = true
            };
            
            System.Diagnostics.Process.Start(processInfo);
            return true;
        }
        catch (Exception)
        {
            // Fallback: Zeige URL zum manuellen Kopieren
            var message = $"PayPal konnte nicht automatisch geöffnet werden.\n\n" +
                         $"Bitte öffnen Sie diesen Link manuell:\n\n{paypalUrl}";
            
            MessageBox.Show(message, "PayPal Link", MessageBoxButton.OK, MessageBoxImage.Information);
            
            // URL in Zwischenablage kopieren
            try
            {
                Clipboard.SetText(paypalUrl);
            }
            catch { }
            
            return true;
        }
    }

    /// <summary>
    /// Validiert Eingabe für Beträge (nur Zahlen, Kommas, Punkte)
    /// </summary>
    private static bool IsValidAmountInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Regex für Beträge: Zahlen mit optional einem Komma oder Punkt und max 2 Nachkommastellen
        var regex = new Regex(@"^\d{1,4}([,.]\d{0,2})?$");
        return regex.IsMatch(input);
    }

    /// <summary>
    /// Zeigt den Dialog an und gibt den gewählten Spendenbetrag zurück
    /// </summary>
    /// <param name="owner">Parent Window</param>
    /// <param name="localizationService">Übersetzungsservice</param>
    /// <returns>Gewählter Betrag oder null wenn abgebrochen</returns>
    public static decimal? ShowDonationDialog(Window owner, LocalizationService localizationService)
    {
        var dialog = new DonationSelectionDialog(localizationService)
        {
            Owner = owner
        };

        var result = dialog.ShowDialog();
        
        if (result == true)
        {
            return dialog._selectedAmount;
        }
        
        return null; // Abgebrochen oder kein Spende
    }
}