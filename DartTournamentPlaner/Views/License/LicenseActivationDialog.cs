using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.License;
using DartTournamentPlaner.Helpers;

namespace DartTournamentPlaner.Views.License;

/// <summary>
/// Moderner Dialog für Lizenzaktivierung im einheitlichen Anwendungsdesign
/// Vollständig programmatisch erstellt für maximale Kompatibilität
/// </summary>
public partial class LicenseActivationDialog : Window
{
    private readonly LocalizationService _localizationService;
    private readonly LicenseManager _licenseManager;
    private bool _isActivating = false;
    
    // UI Controls
    private TextBlock _titleText;
    private TextBlock _subtitleText;
    private TextBox _licenseKeyTextBox;
    private TextBlock _formatInfoText;
    private TextBox _hardwareIdTextBox;
    private Border _progressPanel;
    private TextBlock _progressText;
    private ProgressBar _validationProgress;
    private Button _cancelButton;
    private Button _activateButton;

    public LicenseActivationDialog(LocalizationService localizationService, LicenseManager licenseManager)
    {
        // Initialize XAML first (though we override it immediately)
        InitializeComponent();
        
        _localizationService = localizationService;
        _licenseManager = licenseManager;
        
        InitializeDialog();
        LoadTranslations();
        LoadHardwareId();
    }

    private void InitializeDialog()
    {
        // Window Properties
        Title = "Lizenz aktivieren";
        Width = 520;
        Height = 500;
        MinWidth = 520;
        MinHeight = 500;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.CanResize;
        ShowInTaskbar = false;
        Background = Brushes.Transparent;
        AllowsTransparency = true;
        WindowStyle = WindowStyle.None;
        
        // Create main container
        var mainBorder = new Border
        {
            Background = CreateGradientBrush(Colors.White, Color.FromRgb(248, 250, 252)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(16),
            Margin = new Thickness(10),
            Effect = CreateDropShadow()
        };
        
        var mainGrid = new Grid();
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        
        // Header
        var headerBorder = new Border
        {
            Background = CreateGradientBrush(Color.FromRgb(59, 130, 246), Color.FromRgb(30, 64, 175)),
            CornerRadius = new CornerRadius(16, 16, 0, 0),
            Padding = new Thickness(24, 20, 24, 20)
        };
        
        var headerStack = new StackPanel();
        
        var titlePanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal, 
            Margin = new Thickness(0, 0, 0, 8) 
        };
        titlePanel.Children.Add(new TextBlock 
        { 
            Text = "✨", 
            FontSize = 24, 
            Margin = new Thickness(0, 0, 12, 0),
            VerticalAlignment = VerticalAlignment.Center 
        });
        
        _titleText = new TextBlock
        {
            Text = "Lizenz aktivieren",
            FontSize = 20,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };
        titlePanel.Children.Add(_titleText);
        
        _subtitleText = new TextBlock
        {
            Text = "Geben Sie Ihren Lizenzschlüssel ein, um alle Premium-Features freizuschalten",
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.FromRgb(224, 242, 254)),
            TextWrapping = TextWrapping.Wrap
        };
        
        headerStack.Children.Add(titlePanel);
        headerStack.Children.Add(_subtitleText);
        headerBorder.Child = headerStack;
        
        Grid.SetRow(headerBorder, 0);
        mainGrid.Children.Add(headerBorder);
        
        // Content
        var contentStack = new StackPanel { Margin = new Thickness(24, 24, 24, 20) };
        
        // License Key Input
        contentStack.Children.Add(new TextBlock
        {
            Text = "🔑 Lizenzschlüssel:",
            FontWeight = FontWeights.Medium,
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.FromRgb(55, 65, 81)),
            Margin = new Thickness(0, 0, 0, 8)
        });
        
        _licenseKeyTextBox = new TextBox
        {
            FontFamily = new FontFamily("Consolas"),
            FontSize = 13,
            Margin = new Thickness(0, 0, 0, 16),
            MaxLength = 39,
            Padding = new Thickness(12, 10, 12, 10),
            BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            BorderThickness = new Thickness(1)
        };
        _licenseKeyTextBox.TextChanged += LicenseKeyTextBox_TextChanged;
        contentStack.Children.Add(_licenseKeyTextBox);
        
        // Format Info
        var formatBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(239, 246, 255)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(147, 197, 253)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 0, 16)
        };
        
        var formatStack = new StackPanel();
        var formatTitlePanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal, 
            Margin = new Thickness(0, 0, 0, 4) 
        };
        formatTitlePanel.Children.Add(new TextBlock 
        { 
            Text = "💡", 
            FontSize = 14, 
            Margin = new Thickness(0, 0, 8, 0) 
        });
        formatTitlePanel.Children.Add(new TextBlock
        {
            Text = "Format-Information:",
            FontWeight = FontWeights.SemiBold,
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(30, 64, 175))
        });
        
        _formatInfoText = new TextBlock
        {
            Text = "Lizenzschlüssel bestehen aus 8 Blöcken mit je 4 Zeichen (A-F, 0-9), getrennt durch Bindestriche.",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(30, 64, 175)),
            TextWrapping = TextWrapping.Wrap
        };
        
        formatStack.Children.Add(formatTitlePanel);
        formatStack.Children.Add(_formatInfoText);
        formatBorder.Child = formatStack;
        contentStack.Children.Add(formatBorder);
        
        // Hardware ID Info
        var hardwareBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(240, 253, 244)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(187, 247, 208)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 0, 20)
        };
        
        var hardwareStack = new StackPanel();
        var hardwareTitlePanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal, 
            Margin = new Thickness(0, 0, 0, 4) 
        };
        hardwareTitlePanel.Children.Add(new TextBlock 
        { 
            Text = "🖥️", 
            FontSize = 14, 
            Margin = new Thickness(0, 0, 8, 0) 
        });
        hardwareTitlePanel.Children.Add(new TextBlock
        {
            Text = "Ihre Hardware-ID (für Support):",
            FontWeight = FontWeights.SemiBold,
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(22, 101, 52))
        });
        
        _hardwareIdTextBox = new TextBox
        {
            FontFamily = new FontFamily("Consolas"),
            FontSize = 11,
            IsReadOnly = true,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            Margin = new Thickness(0, 4, 0, 0),
            Foreground = new SolidColorBrush(Color.FromRgb(22, 101, 52)),
            Cursor = Cursors.IBeam,
            ToolTip = "Klicken Sie, um die Hardware-ID zu kopieren"
        };
        _hardwareIdTextBox.MouseLeftButtonUp += HardwareIdTextBox_MouseLeftButtonUp;
        
        hardwareStack.Children.Add(hardwareTitlePanel);
        hardwareStack.Children.Add(_hardwareIdTextBox);
        hardwareBorder.Child = hardwareStack;
        contentStack.Children.Add(hardwareBorder);
        
        // Progress Panel (Initially Hidden)
        _progressPanel = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(254, 243, 199)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(252, 211, 77)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 0, 0, 16),
            Visibility = Visibility.Collapsed
        };
        
        var progressStack = new StackPanel();
        var progressTitlePanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal, 
            Margin = new Thickness(0, 0, 0, 8) 
        };
        progressTitlePanel.Children.Add(new TextBlock 
        { 
            Text = "⏳", 
            FontSize = 16, 
            Margin = new Thickness(0, 0, 8, 0) 
        });
        
        _progressText = new TextBlock
        {
            Text = "Lizenz wird validiert...",
            FontWeight = FontWeights.SemiBold,
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.FromRgb(146, 64, 14))
        };
        progressTitlePanel.Children.Add(_progressText);
        
        _validationProgress = new ProgressBar
        {
            Height = 6,
            IsIndeterminate = true,
            Background = new SolidColorBrush(Color.FromRgb(254, 243, 199)),
            Foreground = new SolidColorBrush(Color.FromRgb(245, 158, 11)),
            BorderThickness = new Thickness(0)
        };
        
        progressStack.Children.Add(progressTitlePanel);
        progressStack.Children.Add(_validationProgress);
        _progressPanel.Child = progressStack;
        contentStack.Children.Add(_progressPanel);
        
        // Action Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 20, 0, 0)
        };
        
        _cancelButton = CreateButton("Abbrechen", false);
        _cancelButton.Click += CancelButton_Click;
        
        _activateButton = CreateButton("Aktivieren", true);
        _activateButton.Click += ActivateButton_Click;
        _activateButton.IsEnabled = false;
        
        buttonPanel.Children.Add(_cancelButton);
        buttonPanel.Children.Add(_activateButton);
        contentStack.Children.Add(buttonPanel);
        
        Grid.SetRow(contentStack, 1);
        mainGrid.Children.Add(contentStack);
        
        mainBorder.Child = mainGrid;
        Content = mainBorder;
    }

    private Button CreateButton(string content, bool isPrimary)
    {
        var button = new Button
        {
            Content = content,
            Padding = new Thickness(16, 10, 16, 10),
            Margin = new Thickness(8),
            FontWeight = FontWeights.Medium,
            FontSize = 14,
            MinWidth = 120,
            Cursor = Cursors.Hand,
            BorderThickness = new Thickness(0)
        };
        
        if (isPrimary)
        {
            button.Background = CreateGradientBrush(Color.FromRgb(59, 130, 246), Color.FromRgb(37, 99, 235));
            button.Foreground = Brushes.White;
        }
        else
        {
            button.Background = CreateGradientBrush(Color.FromRgb(100, 116, 139), Color.FromRgb(71, 85, 105));
            button.Foreground = Brushes.White;
        }
        
        // Simple style without TemplateBinding issues
        button.Style = CreateSimpleButtonStyle(isPrimary);
        
        return button;
    }

    private Style CreateSimpleButtonStyle(bool isPrimary)
    {
        var style = new Style(typeof(Button));
        
        var template = new ControlTemplate(typeof(Button));
        var borderElement = new FrameworkElementFactory(typeof(Border));
        borderElement.Name = "border";
        borderElement.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
        borderElement.SetValue(Border.EffectProperty, CreateDropShadow());
        
        // Set background directly instead of using TemplateBinding
        if (isPrimary)
        {
            borderElement.SetValue(Border.BackgroundProperty, CreateGradientBrush(Color.FromRgb(59, 130, 246), Color.FromRgb(37, 99, 235)));
        }
        else
        {
            borderElement.SetValue(Border.BackgroundProperty, CreateGradientBrush(Color.FromRgb(100, 116, 139), Color.FromRgb(71, 85, 105)));
        }
        
        var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        contentPresenter.SetValue(ContentPresenter.MarginProperty, new Thickness(16, 10, 16, 10));
        
        borderElement.AppendChild(contentPresenter);
        template.VisualTree = borderElement;
        
        style.Setters.Add(new Setter(Button.TemplateProperty, template));
        
        return style;
    }

    private LinearGradientBrush CreateGradientBrush(Color startColor, Color endColor)
    {
        var brush = new LinearGradientBrush();
        brush.StartPoint = new Point(0, 0);
        brush.EndPoint = new Point(0, 1);
        brush.GradientStops.Add(new GradientStop(startColor, 0));
        brush.GradientStops.Add(new GradientStop(endColor, 1));
        return brush;
    }

    private System.Windows.Media.Effects.DropShadowEffect CreateDropShadow()
    {
        return new System.Windows.Media.Effects.DropShadowEffect
        {
            Color = Colors.Black,
            Direction = 270,
            ShadowDepth = 4,
            BlurRadius = 16,
            Opacity = 0.08
        };
    }

    private void LoadTranslations()
    {
        Title = _localizationService.GetString("ActivateLicense") ?? "Lizenz aktivieren";
        _titleText.Text = _localizationService.GetString("ActivateLicense") ?? "Lizenz aktivieren";
        _subtitleText.Text = _localizationService.GetString("LicenseActivationMessage") ?? 
            "Geben Sie Ihren Lizenzschlüssel ein, um alle Premium-Features freizuschalten";
            
        _formatInfoText.Text = _localizationService.GetString("LicenseKeyFormatInfo") ?? 
            "Lizenzschlüssel bestehen aus 8 Blöcken mit je 4 Zeichen (A-F, 0-9), getrennt durch Bindestriche.";
            
        _cancelButton.Content = _localizationService.GetString("Cancel") ?? "Abbrechen";
        _activateButton.Content = _localizationService.GetString("ActivateLicense") ?? "Aktivieren";
        _progressText.Text = _localizationService.GetString("ValidatingLicense") ?? "Lizenz wird validiert...";
    }

    private void LoadHardwareId()
    {
        try
        {
            var hardwareId = LicenseManager.GenerateHardwareId();
            _hardwareIdTextBox.Text = hardwareId;
        }
        catch (Exception ex)
        {
            _hardwareIdTextBox.Text = $"Fehler beim Laden der Hardware-ID: {ex.Message}";
        }
    }

    private void LicenseKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var input = textBox.Text;
            var formatted = FormatLicenseKey(input);
            
            if (formatted != input)
            {
                var cursorPosition = textBox.CaretIndex;
                textBox.Text = formatted;
                
                // Cursor-Position anpassen
                var newPosition = Math.Min(cursorPosition + (formatted.Length - input.Length), formatted.Length);
                textBox.CaretIndex = Math.Max(0, newPosition);
            }
            
            // Aktivieren-Button nur verfügbar wenn Format gültig ist
            _activateButton.IsEnabled = IsValidLicenseKeyFormat(formatted) && !_isActivating;
        }
    }

    private string FormatLicenseKey(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Nur Hex-Zeichen beibehalten und zu Großbuchstaben konvertieren
        var hexOnly = Regex.Replace(input.ToUpperInvariant(), @"[^0-9A-F]", "");
        
        // Maximal 32 Zeichen (8 * 4)
        if (hexOnly.Length > 32)
            hexOnly = hexOnly.Substring(0, 32);
        
        // Bindestriche einfügen alle 4 Zeichen
        var formatted = string.Empty;
        for (int i = 0; i < hexOnly.Length; i++)
        {
            if (i > 0 && i % 4 == 0)
                formatted += "-";
            formatted += hexOnly[i];
        }
        
        return formatted;
    }

    private bool IsValidLicenseKeyFormat(string licenseKey)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            return false;

        // Erwartetes Format: BDF6-192D-E8BE-4178-B160-C6C3-6018-0FE3 (39 Zeichen total)
        var pattern = @"^[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}$";
        return Regex.IsMatch(licenseKey, pattern);
    }

    private void HardwareIdTextBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(_hardwareIdTextBox.Text))
            {
                Clipboard.SetText(_hardwareIdTextBox.Text);
                
                // Visual Feedback
                var originalText = _hardwareIdTextBox.Text;
                _hardwareIdTextBox.Text = "📋 Hardware-ID kopiert!";
                _hardwareIdTextBox.Foreground = Brushes.Green;
                
                // Nach 2 Sekunden zurücksetzen
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(2)
                };
                timer.Tick += (s, args) =>
                {
                    _hardwareIdTextBox.Text = originalText;
                    _hardwareIdTextBox.Foreground = new SolidColorBrush(Color.FromRgb(22, 101, 52));
                    timer.Stop();
                };
                timer.Start();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error copying Hardware-ID: {ex.Message}");
        }
    }

    private async void ActivateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isActivating)
            return;

        var licenseKey = _licenseKeyTextBox.Text?.Trim();
        if (string.IsNullOrEmpty(licenseKey) || !IsValidLicenseKeyFormat(licenseKey))
        {
            ShowError(_localizationService.GetString("InvalidLicenseKeyFormat") ?? 
                "Ungültiges Lizenzschlüssel-Format. Bitte überprüfen Sie die Eingabe.");
            return;
        }

        await ActivateLicenseAsync(licenseKey);
    }

    private async Task ActivateLicenseAsync(string licenseKey)
    {
        try
        {
            _isActivating = true;
            SetProgressState(true);
            
            var result = await _licenseManager.ActivateLicenseAsync(licenseKey);
            
            SetProgressState(false);
            
            if (result.IsValid)
            {
                // Erfolg - zeige Erfolgs-Dialog
                DialogResult = true;
                ShowSuccessDialog(result);
            }
            else
            {
                // Fehler anzeigen
                var errorMessage = GetUserFriendlyErrorMessage(result);
                ShowError(errorMessage);
            }
        }
        catch (Exception ex)
        {
            SetProgressState(false);
            ShowError($"Unerwarteter Fehler bei der Aktivierung: {ex.Message}");
        }
        finally
        {
            _isActivating = false;
            _activateButton.IsEnabled = IsValidLicenseKeyFormat(_licenseKeyTextBox.Text) && !_isActivating;
        }
    }

    private void ShowSuccessDialog(Models.License.LicenseValidationResult result)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🎉 Attempting to show modern success dialog...");
            
            // Zuerst den Erfolgs-Dialog anzeigen
            var successDialog = new LicenseActivationSuccessDialog(_localizationService, result);
            
            // Owner nur setzen wenn this Window noch gültig ist
            if (this.IsLoaded)
            {
                try
                {
                    successDialog.Owner = this;
                    System.Diagnostics.Debug.WriteLine("✅ Owner set successfully");
                }
                catch (Exception ownerEx)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Could not set owner: {ownerEx.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Parent window not loaded - skipping owner relationship");
            }
            
            System.Diagnostics.Debug.WriteLine("📱 Showing success dialog...");
            successDialog.ShowDialog();
            System.Diagnostics.Debug.WriteLine("✅ Success dialog closed normally");
            
            // Dann das Hauptdialog mit Erfolg schließen
            try
            {
                DialogResult = true;
                System.Diagnostics.Debug.WriteLine("✅ DialogResult set to true");
            }
            catch (InvalidOperationException ex)
            {
                // Fallback: Einfach schließen ohne DialogResult zu setzen
                System.Diagnostics.Debug.WriteLine($"❌ Could not set DialogResult: {ex.Message}");
            }
            
            Close();
            System.Diagnostics.Debug.WriteLine("🔚 Main activation dialog closed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error showing modern success dialog: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"📋 Stack trace: {ex.StackTrace}");
            
            // Fallback zu einfacher MessageBox
            System.Diagnostics.Debug.WriteLine("🔄 Falling back to MessageBox...");
            var message = BuildSuccessMessage(result);
            TournamentDialogHelper.ShowInformation(message, "Lizenz erfolgreich aktiviert", _localizationService, this);
            
            try
            {
                DialogResult = true;
            }
            catch (InvalidOperationException)
            {
                // Fallback: Einfach schließen ohne DialogResult zu setzen
                System.Diagnostics.Debug.WriteLine("❌ Could not set DialogResult in fallback - closing normally");
            }
            
            Close();
        }
    }

    private void ShowError(string message)
    {
        TournamentDialogHelper.ShowError(message, _localizationService.GetString("Error") ?? "Fehler", _localizationService, this);
    }

    private void SetProgressState(bool isActive)
    {
        _progressPanel.Visibility = isActive ? Visibility.Visible : Visibility.Collapsed;
        _activateButton.IsEnabled = !isActive && IsValidLicenseKeyFormat(_licenseKeyTextBox.Text);
        _licenseKeyTextBox.IsEnabled = !isActive;
    }

    private string BuildSuccessMessage(Models.License.LicenseValidationResult result)
    {
        var message = "✅ Lizenz erfolgreich aktiviert!\n\n";
        
        if (result.Data != null)
        {
            message += $"📄 Kunde: {result.Data.CustomerName}\n";
            message += $"📦 Produkt: {result.Data.ProductName}\n";
            
            if (result.Data.ExpiresAt.HasValue)
            {
                message += $"📅 Gültig bis: {result.Data.ExpiresAt.Value:dd.MM.yyyy HH:mm}\n";
            }
            else
            {
                message += "📅 Gültig bis: Unbegrenzt\n";
            }
            
            if (result.Data.Features != null && result.Data.Features.Length > 0)
            {
                message += "\n🚀 Aktivierte Features:\n";
                foreach (var feature in result.Data.Features)
                {
                    message += $"  • {GetFeatureDisplayName(feature)}\n";
                }
            }
        }
        
        return message;
    }

    private string GetUserFriendlyErrorMessage(Models.License.LicenseValidationResult result)
    {
        return result.ErrorType switch
        {
            Models.License.LicenseErrorType.LicenseNotFound => 
                "❌ Lizenzschlüssel nicht gefunden oder ungültig.\n\nBitte überprüfen Sie Ihren Lizenzschlüssel.",
            
            Models.License.LicenseErrorType.LicenseExpired => 
                "⏰ Diese Lizenz ist abgelaufen.\n\nBitte kontaktieren Sie den Support für eine Erneuerung.",
            
            Models.License.LicenseErrorType.LicenseInactive => 
                "🚫 Diese Lizenz ist inaktiv.\n\nBitte kontaktieren Sie den Support.",
            
            Models.License.LicenseErrorType.MaxActivationsReached => 
                "🔒 Das Aktivierungslimit für diese Lizenz wurde erreicht.\n\nBitte kontaktieren Sie den Support, um das Limit zurückzusetzen.",
            
            Models.License.LicenseErrorType.InvalidFormat => 
                "📝 Ungültiges Lizenzschlüssel-Format.\n\nBitte überprüfen Sie die Eingabe.",
            
            Models.License.LicenseErrorType.NetworkError => 
                "🌐 Netzwerkfehler beim Kontaktieren des Lizenzservers.\n\nBitte überprüfen Sie Ihre Internetverbindung und versuchen Sie es erneut.",
            
            Models.License.LicenseErrorType.ServerError => 
                "⚠️ Server-Fehler beim Validieren der Lizenz.\n\nBitte versuchen Sie es später erneut.",
            
            _ => result.Message ?? "❌ Unbekannter Fehler bei der Lizenzaktivierung."
        };
    }

    private static string GetFeatureDisplayName(string featureId)
    {
        return featureId switch
        {
            "tournament_management" => "Turnier-Management",
            "player_tracking" => "Spieler-Verfolgung", 
            "statistics" => "Erweiterte Statistiken",
            "api_access" => "API-Zugang",
            "hub_integration" => "Hub-Integration",
            "enhanced_printing" => "Erweiterte Druckfunktionen",
            "multi_tournament" => "Multi-Turnier",
            "advanced_reporting" => "Erweiterte Berichte",
            "custom_themes" => "Benutzerdefinierte Themes",
            "premium_support" => "Premium-Support",
            "tournament_overview" => "Tournament Overview",  // NEU
            _ => featureId
        };
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DialogResult = false;
        }
        catch (InvalidOperationException)
        {
            // Fallback: Einfach schließen ohne DialogResult zu setzen
            System.Diagnostics.Debug.WriteLine("Could not set DialogResult on cancel - closing normally");
        }
        
        Close();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
 
     /// <summary>
     /// Zeigt den Dialog als Modal Window an
     /// </summary>
    public static async Task<bool> ShowDialogAsync(Window owner, LocalizationService localizationService, LicenseManager licenseManager)
    {
        try
        {
            var dialog = new LicenseActivationDialog(localizationService, licenseManager);
            
            // Owner setzen falls verfügbar
            if (owner != null)
            {
                dialog.Owner = owner;
            }
            
            // Als Modal Dialog anzeigen
            var result = dialog.ShowDialog();
            return result == true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing license activation dialog: {ex.Message}");
            return false;
        }
    }
}