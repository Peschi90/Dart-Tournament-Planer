using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views.License;

/// <summary>
/// Erfolg-Dialog für die Lizenzaktivierung im modernen Design
/// Vollständig programmatisch erstellt für maximale Kompatibilität
/// </summary>
public class LicenseActivationSuccessDialog : Window
{
    private readonly LocalizationService _localizationService;
    private readonly Models.License.LicenseValidationResult _result;
    
    // UI Controls
    private TextBlock _titleText;
    private TextBlock _subtitleText;
    private TextBlock _customerNameText;
    private TextBlock _productNameText;
    private TextBlock _expiryText;
    private TextBlock _remainingActivationsLabel;
    private TextBlock _remainingActivationsText;
    private StackPanel _featuresPanel;
    private Border _activationWarning;
    private TextBlock _warningText;
    private Button _continueButton;

    public LicenseActivationSuccessDialog(LocalizationService localizationService, Models.License.LicenseValidationResult result)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🚀 Initializing LicenseActivationSuccessDialog...");
            
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _result = result ?? throw new ArgumentNullException(nameof(result));
            
            System.Diagnostics.Debug.WriteLine("📦 Services initialized, starting dialog initialization...");
            InitializeDialog();
            System.Diagnostics.Debug.WriteLine("🎨 Dialog UI initialized");
            
            LoadTranslations();
            System.Diagnostics.Debug.WriteLine("🌍 Translations loaded");
            
            LoadLicenseData();
            System.Diagnostics.Debug.WriteLine("📊 License data loaded");
            
            System.Diagnostics.Debug.WriteLine("✅ LicenseActivationSuccessDialog fully initialized");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error in LicenseActivationSuccessDialog constructor: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"📋 Stack trace: {ex.StackTrace}");
            throw; // Re-throw to let caller handle it
        }
    }

    private void InitializeDialog()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🏗️ Starting dialog initialization...");
            
            // Window Properties - Use safer settings first
            Title = "Lizenz erfolgreich aktiviert";
            Width = 550;
            Height = 550;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            
            // Try modern styling, but fall back if it causes issues
            try
            {
                Background = Brushes.Transparent;
                AllowsTransparency = true;
                WindowStyle = WindowStyle.None;
                System.Diagnostics.Debug.WriteLine("✅ Modern window styling applied");
            }
            catch (Exception styleEx)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Modern styling failed, using standard: {styleEx.Message}");
                Background = Brushes.White;
                AllowsTransparency = false;
                WindowStyle = WindowStyle.SingleBorderWindow;
            }
            
            System.Diagnostics.Debug.WriteLine("📐 Window properties set, creating UI elements...");
            
            // Create main container
            var mainBorder = new Border
            {
                Background = CreateGradientBrush(Colors.White, Color.FromRgb(240, 253, 244)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(187, 247, 208)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(16),
                Margin = new Thickness(10),
                Effect = CreateDropShadow()
            };
            
            System.Diagnostics.Debug.WriteLine("🎨 Main border created");
            
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            
            // Header
            var headerBorder = new Border
            {
                Background = CreateGradientBrush(Color.FromRgb(16, 185, 129), Color.FromRgb(5, 150, 105)),
                CornerRadius = new CornerRadius(14, 14, 0, 0),
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
                Text = "🎉", 
                FontSize = 28, 
                Margin = new Thickness(0, 0, 16, 0),
                VerticalAlignment = VerticalAlignment.Center 
            });
            
            var titleSubStack = new StackPanel();
            _titleText = new TextBlock
            {
                Text = "Lizenz erfolgreich aktiviert!",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            _subtitleText = new TextBlock
            {
                Text = "Alle Premium-Features sind jetzt verfügbar",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(209, 250, 229)),
                Margin = new Thickness(0, 4, 0, 0)
            };
            
            titleSubStack.Children.Add(_titleText);
            titleSubStack.Children.Add(_subtitleText);
            titlePanel.Children.Add(titleSubStack);
            
            headerStack.Children.Add(titlePanel);
            headerBorder.Child = headerStack;
            
            Grid.SetRow(headerBorder, 0);
            mainGrid.Children.Add(headerBorder);
            
            System.Diagnostics.Debug.WriteLine("📱 Header created");
            
            // Content ScrollViewer
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(24, 20, 24, 20)
            };
            
            var contentStack = new StackPanel();
            
            // License Information Card
            var licenseCard = CreateCard("📄", "Lizenz-Details:");
            var licenseGrid = new Grid();
            licenseGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            licenseGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            licenseGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            licenseGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            licenseGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            licenseGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            // Kunde
            licenseGrid.Children.Add(CreateLabel("👤 Kunde:", 0));
            _customerNameText = CreateValueText("Kunde Name", 0);
            licenseGrid.Children.Add(_customerNameText);
            
            // Produkt
            licenseGrid.Children.Add(CreateLabel("📦 Produkt:", 1));
            _productNameText = CreateValueText("Dart Tournament Planner", 1);
            licenseGrid.Children.Add(_productNameText);
            
            // Gültigkeit
            licenseGrid.Children.Add(CreateLabel("📅 Gültig bis:", 2));
            _expiryText = CreateValueText("Unbegrenzt", 2);
            licenseGrid.Children.Add(_expiryText);
            
            // Verbleibende Aktivierungen
            _remainingActivationsLabel = CreateLabel("🔄 Verbleibende:", 3);
            _remainingActivationsLabel.Visibility = Visibility.Collapsed;
            licenseGrid.Children.Add(_remainingActivationsLabel);
            
            _remainingActivationsText = CreateValueText("5 Aktivierungen", 3);
            _remainingActivationsText.Visibility = Visibility.Collapsed;
            licenseGrid.Children.Add(_remainingActivationsText);
            
            licenseCard.Child = licenseGrid;
            contentStack.Children.Add(licenseCard);
            
            System.Diagnostics.Debug.WriteLine("📊 License info card created");
            
            // Features Card
            var featuresCard = CreateCard("🚀", "Aktivierte Features:");
            _featuresPanel = new StackPanel();
            featuresCard.Child = _featuresPanel;
            contentStack.Children.Add(featuresCard);
            
            System.Diagnostics.Debug.WriteLine("🚀 Features card created");
            
            // Warning Panel (Initially Hidden)
            _activationWarning = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(254, 243, 199)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(245, 158, 11)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 16),
                Visibility = Visibility.Collapsed
            };
            
            var warningStack = new StackPanel();
            var warningTitlePanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                Margin = new Thickness(0, 0, 0, 8) 
            };
            warningTitlePanel.Children.Add(new TextBlock 
            { 
                Text = "⚠️", 
                FontSize = 16, 
                Margin = new Thickness(0, 0, 8, 0) 
            });
            warningTitlePanel.Children.Add(new TextBlock
            {
                Text = "Aktivierungs-Hinweis:",
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(146, 64, 14))
            });
            
            _warningText = new TextBlock
            {
                Text = "Sie haben nur noch wenige Aktivierungen für diese Lizenz übrig.",
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(146, 64, 14)),
                TextWrapping = TextWrapping.Wrap
            };
            
            warningStack.Children.Add(warningTitlePanel);
            warningStack.Children.Add(_warningText);
            _activationWarning.Child = warningStack;
            contentStack.Children.Add(_activationWarning);
            
            System.Diagnostics.Debug.WriteLine("⚠️ Warning panel created");
            
            // Continue Button
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 16, 0, 0)
            };
            
            _continueButton = CreateSuccessButton("🎊 Perfekt, weiter geht's!");
            _continueButton.Click += ContinueButton_Click;
            buttonPanel.Children.Add(_continueButton);
            contentStack.Children.Add(buttonPanel);
            
            System.Diagnostics.Debug.WriteLine("🔘 Button created");
            
            scrollViewer.Content = contentStack;
            Grid.SetRow(scrollViewer, 1);
            mainGrid.Children.Add(scrollViewer);
            
            mainBorder.Child = mainGrid;
            Content = mainBorder;
            
            System.Diagnostics.Debug.WriteLine("✅ Dialog initialization completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error in InitializeDialog: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"📋 Stack trace: {ex.StackTrace}");
            throw; // Re-throw to let caller handle it
        }
    }

    private Border CreateCard(string icon, string title)
    {
        var card = new Border
        {
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(209, 250, 229)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 0, 0, 16),
            Effect = CreateDropShadow()
        };
        
        var stack = new StackPanel();
        var titlePanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal, 
            Margin = new Thickness(0, 0, 0, 12) 
        };
        titlePanel.Children.Add(new TextBlock 
        { 
            Text = icon, 
            FontSize = 16, 
            Margin = new Thickness(0, 0, 8, 0) 
        });
        titlePanel.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeights.SemiBold,
            FontSize = 15,
            Foreground = new SolidColorBrush(Color.FromRgb(6, 95, 70))
        });
        
        stack.Children.Add(titlePanel);
        card.Child = stack;
        
        return card;
    }

    private TextBlock CreateLabel(string text, int row)
    {
        var label = new TextBlock
        {
            Text = text,
            FontWeight = FontWeights.Medium,
            Foreground = new SolidColorBrush(Color.FromRgb(55, 65, 81)),
            Margin = new Thickness(0, 4, 16, 4),
            VerticalAlignment = VerticalAlignment.Top
        };
        
        Grid.SetRow(label, row);
        Grid.SetColumn(label, 0);
        return label;
    }

    private TextBlock CreateValueText(string text, int row)
    {
        var valueText = new TextBlock
        {
            Text = text,
            Foreground = new SolidColorBrush(Color.FromRgb(6, 95, 70)),
            FontWeight = FontWeights.Medium,
            Margin = new Thickness(0, 4, 0, 4),
            TextWrapping = TextWrapping.Wrap
        };
        
        Grid.SetRow(valueText, row);
        Grid.SetColumn(valueText, 1);
        return valueText;
    }

    private Button CreateSuccessButton(string content)
    {
        var button = new Button
        {
            Content = content,
            Padding = new Thickness(20, 12, 20, 12),
            Margin = new Thickness(8),
            FontWeight = FontWeights.Medium,
            FontSize = 14,
            MinWidth = 140,
            Cursor = Cursors.Hand,
            BorderThickness = new Thickness(0),
            Background = CreateGradientBrush(Color.FromRgb(16, 185, 129), Color.FromRgb(5, 150, 105)),
            Foreground = Brushes.White
        };
        
        // Simple style for success button
        button.Style = CreateSimpleSuccessButtonStyle();
        
        return button;
    }

    private Style CreateSimpleSuccessButtonStyle()
    {
        var style = new Style(typeof(Button));
        
        var template = new ControlTemplate(typeof(Button));
        var borderElement = new FrameworkElementFactory(typeof(Border));
        borderElement.Name = "border";
        borderElement.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
        borderElement.SetValue(Border.EffectProperty, CreateDropShadow());
        borderElement.SetValue(Border.BackgroundProperty, CreateGradientBrush(Color.FromRgb(16, 185, 129), Color.FromRgb(5, 150, 105)));
        
        var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        contentPresenter.SetValue(ContentPresenter.MarginProperty, new Thickness(20, 12, 20, 12));
        
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
        Title = _localizationService.GetString("LicenseActivatedSuccessfully") ?? "Lizenz erfolgreich aktiviert!";
        _titleText.Text = _localizationService.GetString("LicenseActivatedSuccessfully") ?? "Lizenz erfolgreich aktiviert!";
        _subtitleText.Text = _localizationService.GetString("LicenseActivationSuccessSubtitle") ?? 
            "Alle Premium-Features sind jetzt verfügbar";
        _continueButton.Content = _localizationService.GetString("Continue") ?? "🎊 Perfekt, weiter geht's!";
    }

    private void LoadLicenseData()
    {
        if (_result?.Data == null)
        {
            ShowGenericSuccess();
            return;
        }

        var data = _result.Data;
        
        // Kunde
        _customerNameText.Text = string.IsNullOrEmpty(data.CustomerName) ? "Unbekannt" : data.CustomerName;
        
        // Produkt
        _productNameText.Text = string.IsNullOrEmpty(data.ProductName) ? "Dart Tournament Planner" : data.ProductName;
        
        // Gültigkeit
        if (data.ExpiresAt.HasValue)
        {
            _expiryText.Text = data.ExpiresAt.Value.ToString("dd.MM.yyyy HH:mm");
        }
        else
        {
            _expiryText.Text = _localizationService.GetString("Unlimited") ?? "Unbegrenzt";
            _expiryText.Foreground = Brushes.Green;
        }
        
        // Verbleibende Aktivierungen
        if (data.RemainingActivations.HasValue && data.RemainingActivations.Value >= 0)
        {
            _remainingActivationsLabel.Visibility = Visibility.Visible;
            _remainingActivationsText.Visibility = Visibility.Visible;
            _remainingActivationsText.Text = $"{data.RemainingActivations.Value} Aktivierungen";
            
            // Warnung bei wenigen Aktivierungen anzeigen
            if (data.RemainingActivations.Value <= 2)
            {
                ShowActivationWarning(data.RemainingActivations.Value);
            }
        }
        
        // Features laden
        LoadFeatures(data.Features);
    }

    private void LoadFeatures(string[]? features)
    {
        _featuresPanel.Children.Clear();
        
        if (features == null || features.Length == 0)
        {
            var noFeaturesText = new TextBlock
            {
                Text = _localizationService.GetString("NoSpecificFeatures") ?? "Alle Standard-Features verfügbar",
                FontSize = 13,
                Foreground = Brushes.Gray,
                FontStyle = FontStyles.Italic
            };
            _featuresPanel.Children.Add(noFeaturesText);
            return;
        }
        
        foreach (var feature in features)
        {
            var featurePanel = CreateFeatureItem(feature);
            _featuresPanel.Children.Add(featurePanel);
        }
    }

    private StackPanel CreateFeatureItem(string featureId)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 4, 0, 4)
        };
        
        // Feature Icon
        var icon = new TextBlock
        {
            Text = GetFeatureIcon(featureId),
            FontSize = 14,
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        
        // Feature Name
        var name = new TextBlock
        {
            Text = GetFeatureDisplayName(featureId),
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(6, 95, 70)),
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeights.Medium
        };
        
        panel.Children.Add(icon);
        panel.Children.Add(name);
        
        return panel;
    }

    private void ShowActivationWarning(int remainingActivations)
    {
        _activationWarning.Visibility = Visibility.Visible;
        
        if (remainingActivations == 0)
        {
            _warningText.Text = _localizationService.GetString("LastActivationWarning") ?? 
                "Dies war Ihre letzte verfügbare Aktivierung für diese Lizenz. " +
                "Kontaktieren Sie den Support, falls Sie die Software auf zusätzlichen Computern installieren müssen.";
        }
        else
        {
            _warningText.Text = string.Format(
                _localizationService.GetString("FewActivationsWarning") ?? 
                "Sie haben noch {0} Aktivierung(en) für diese Lizenz übrig. " +
                "Planen Sie weitere Installationen sorgfältig.", 
                remainingActivations);
        }
    }

    private void ShowGenericSuccess()
    {
        // Fallback für den Fall, dass keine Lizenz-Details verfügbar sind
        _customerNameText.Text = "Unbekannt";
        _productNameText.Text = "Dart Tournament Planner";
        _expiryText.Text = "Unbekannt";
        
        var genericText = new TextBlock
        {
            Text = _localizationService.GetString("GenericLicenseActivated") ?? 
                "Ihre Lizenz wurde erfolgreich aktiviert.",
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(6, 95, 70)),
            FontWeight = FontWeights.Medium
        };
        _featuresPanel.Children.Add(genericText);
    }

    private static string GetFeatureIcon(string featureId)
    {
        return featureId switch
        {
            "tournament_management" => "🏆",
            "player_tracking" => "👥",
            "statistics" => "📊",
            "api_access" => "🔗",
            "hub_integration" => "🌐",
            "enhanced_printing" => "🖨️",
            "multi_tournament" => "🎯",
            "advanced_reporting" => "📈",
            "custom_themes" => "🎨",
            "premium_support" => "💎",
            _ => "✨"
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
            "multi_tournament" => "Multi-Turnier Support",
            "advanced_reporting" => "Erweiterte Berichte",
            "custom_themes" => "Benutzerdefinierte Themes",
            "premium_support" => "Premium-Support",
            _ => featureId
        };
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DialogResult = true;
        }
        catch (InvalidOperationException)
        {
            // Fallback: Einfach schließen ohne DialogResult zu setzen
            System.Diagnostics.Debug.WriteLine("Could not set DialogResult on continue - closing normally");
        }
        
        Close();
    }

    /// <summary>
    /// Zeigt den Dialog als Modal Window an
    /// </summary>
    public static void ShowDialog(Window owner, LocalizationService localizationService, Models.License.LicenseValidationResult result)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🎯 ShowDialog called - creating success dialog instance...");
            
            var dialog = new LicenseActivationSuccessDialog(localizationService, result);
            System.Diagnostics.Debug.WriteLine("✅ Success dialog instance created");
            
            if (owner != null && owner.IsLoaded)
            {
                try
                {
                    dialog.Owner = owner;
                    System.Diagnostics.Debug.WriteLine("👨‍👩‍👧‍👦 Owner relationship established");
                }
                catch (Exception ownerEx)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Could not set owner: {ownerEx.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Owner not available or not loaded");
            }
            
            System.Diagnostics.Debug.WriteLine("📱 Calling ShowDialog()...");
            dialog.ShowDialog();
            System.Diagnostics.Debug.WriteLine("✅ ShowDialog() completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error in ShowDialog: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"📋 Stack trace: {ex.StackTrace}");
            
            // Fallback zu einfacher MessageBox
            System.Diagnostics.Debug.WriteLine("🔄 Using MessageBox fallback...");
            MessageBox.Show(
                "Lizenz erfolgreich aktiviert!\n\nAlle Premium-Features sind jetzt verfügbar.",
                "Erfolgreich",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}