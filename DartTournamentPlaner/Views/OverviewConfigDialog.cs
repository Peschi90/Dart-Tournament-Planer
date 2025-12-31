using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views;

public partial class OverviewConfigDialog : Window
{
    private readonly LocalizationService _localizationService;
    
    // Direct references to avoid NameScope issues
    private TextBox _classIntervalTextBox;
    private TextBox _subTabIntervalTextBox;
    private CheckBox _showOnlyActiveCheckBox;
    
    public int ClassInterval { get; set; }
    public int SubTabInterval { get; set; }
    public bool ShowOnlyActiveClasses { get; set; }

    private Brush GetBrush(string key, Brush fallback)
    {
        if (TryFindResource(key) is Brush b)
            return b;
        if (Application.Current != null && Application.Current.TryFindResource(key) is Brush appBrush)
            return appBrush;
        return fallback;
    }

    public OverviewConfigDialog()
    {
        // ✅ KORRIGIERT: Hole LocalizationService aus Application static property
        _localizationService = App.LocalizationService ?? throw new InvalidOperationException("LocalizationService not available");
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Title = _localizationService.GetString("OverviewConfiguration");
        Width = 520;
        Height = 480;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        
        // Moderner Glaseffekt-Hintergrund
        Background = GetBrush("DialogSurfaceGradient", new LinearGradientBrush(
            Color.FromRgb(248, 250, 252),
            Color.FromRgb(241, 245, 249), 90));

        // Hauptcontainer mit modernem Card-Design
        var mainBorder = new Border
        {
            Background = GetBrush("DialogSurfaceGradient", new LinearGradientBrush(
                Color.FromRgb(255, 255, 255),
                Color.FromRgb(253, 253, 253), 90)),
            CornerRadius = new CornerRadius(16),
            Margin = new Thickness(16),
            Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 270,
                ShadowDepth = 8,
                BlurRadius = 24,
                Opacity = 0.12
            }
        };

        var grid = new Grid { Margin = new Thickness(32) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) }); // Spacer
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Main interval setting
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Checkbox
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Info
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Spacer
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // ✅ KORRIGIERT: Header mit korrekten Padding-Parametern
        var headerBorder = new Border
        {
            Background = GetBrush("DialogInfoGradient", new LinearGradientBrush(
                Color.FromRgb(30, 64, 175),
                Color.FromRgb(30, 58, 138), 90)),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(20, 16, 20, 16),
            Margin = new Thickness(0, 0, 0, 0),
            Effect = new DropShadowEffect
            {
                Color = Color.FromRgb(30, 64, 175),
                Direction = 270,
                ShadowDepth = 0,
                BlurRadius = 12,
                Opacity = 0.3
            }
        };

        var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
        
        var iconText = new TextBlock
        {
            Text = "⚙",
            FontSize = 24,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 12, 0),
            VerticalAlignment = VerticalAlignment.Center,
            FontFamily = new FontFamily("Segoe UI Emoji, Segoe UI Symbol, Segoe UI")
        };

        var titleBlock = new TextBlock
        {
            Text = _localizationService.GetString("TournamentOverviewConfiguration") ?? "Tournament Overview Konfiguration",
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };

        headerPanel.Children.Add(iconText);
        headerPanel.Children.Add(titleBlock);
        headerBorder.Child = headerPanel;

        Grid.SetRow(headerBorder, 0);
        grid.Children.Add(headerBorder);

        // Main Tab Interval Section
        var mainSection = CreateInputSection(
            _localizationService.GetString("TabDisplayTime") ?? "Zeit pro Tab:",
            out _subTabIntervalTextBox,
            _localizationService.GetString("Seconds") ?? "Sekunden",
            "Wie lange jeder Tab angezeigt wird, bevor zum nächsten gewechselt wird"
        );
        Grid.SetRow(mainSection, 2);
        grid.Children.Add(mainSection);

        // Backup interval - hidden but kept for compatibility
        _classIntervalTextBox = new TextBox
        {
            Visibility = Visibility.Collapsed,
            Text = "5"
        };

        // ✅ KORRIGIERT: Show Only Active Classes Checkbox mit korrekten Padding-Parametern
        var checkboxBorder = new Border
        {
            Background = GetBrush("DialogWarningGradient", new LinearGradientBrush(
                Color.FromRgb(239, 246, 255),
                Color.FromRgb(219, 234, 254), 90)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 12, 16, 12),
            Margin = new Thickness(0, 20, 0, 0)
        };

        _showOnlyActiveCheckBox = new CheckBox
        {
            Content = _localizationService.GetString("ShowOnlyActiveClassesText") ?? "Nur aktive Klassen anzeigen",
            FontSize = 14,
            FontWeight = FontWeights.Medium,
            Foreground = new SolidColorBrush(Color.FromRgb(30, 64, 175))
        };

        checkboxBorder.Child = _showOnlyActiveCheckBox;
        Grid.SetRow(checkboxBorder, 3);
        grid.Children.Add(checkboxBorder);

        // Info Text
        var infoText = new TextBlock
        {
            Text = "💡 Konfiguration:\n" +
                   "• Jeder Tab wird für die angegebene Zeit angezeigt\n" +
                   "• Nach allen Tabs einer Klasse wird zur nächsten Klasse gewechselt\n" +
                   "• Scrollen läuft parallel und blockiert den Tab-Wechsel nicht\n" +
                   "• Perfekt für Turnier-Präsentationen auf großen Bildschirmen\n" +
                   "• Statistik-Tabs werden automatisch in die Übersicht integriert",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 20, 0, 0),
            Foreground = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
            FontSize = 12,
            LineHeight = 18
        };
        Grid.SetRow(infoText, 4);
        grid.Children.Add(infoText);

        // ✅ KORRIGIERT: Buttons mit korrekter Funktionalität
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 20, 0, 0)
        };

        var cancelButton = CreateModernButton(
            _localizationService.GetString("Cancel") ?? "Abbrechen",
            Color.FromRgb(100, 116, 139),
            Color.FromRgb(71, 85, 105));
        cancelButton.IsCancel = true;
        cancelButton.Margin = new Thickness(0, 0, 12, 0);
        cancelButton.Click += CancelButton_Click;

        var applyButton = CreateModernButton(
            _localizationService.GetString("Apply") ?? "Anwenden",
            Color.FromRgb(59, 130, 246),
            Color.FromRgb(37, 99, 235));
        applyButton.Margin = new Thickness(0, 0, 12, 0);
        applyButton.Click += ApplyButton_Click;

        var saveButton = CreateModernButton(
            _localizationService.GetString("Save") ?? "Speichern",
            Color.FromRgb(34, 197, 94),
            Color.FromRgb(22, 163, 74));
        saveButton.IsDefault = true;
        saveButton.Click += SaveButton_Click;

        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(applyButton);
        buttonPanel.Children.Add(saveButton);

        Grid.SetRow(buttonPanel, 6);
        grid.Children.Add(buttonPanel);

        mainBorder.Child = grid;
        Content = mainBorder;

        Loaded += Dialog_Loaded;
    }

    private void Dialog_Loaded(object sender, RoutedEventArgs e)
    {
        // Set initial values
        _subTabIntervalTextBox.Text = SubTabInterval.ToString();
        _classIntervalTextBox.Text = SubTabInterval.ToString(); // Backup für Kompatibilität
        _showOnlyActiveCheckBox.IsChecked = ShowOnlyActiveClasses;

        _subTabIntervalTextBox.Focus();
        _subTabIntervalTextBox.SelectAll();
    }

    private Border CreateInputSection(string labelText, out TextBox textBox, string suffixText, string tooltip)
    {
        var sectionBorder = new Border
        {
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(20, 16, 20, 16),
            Margin = new Thickness(0, 0, 0, 0),
            Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 270,
                ShadowDepth = 2,
                BlurRadius = 8,
                Opacity = 0.06
            }
        };

        var sectionGrid = new Grid();
        sectionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
        sectionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var label = new TextBlock
        {
            Text = labelText,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 14,
            FontWeight = FontWeights.Medium,
            Foreground = new SolidColorBrush(Color.FromRgb(55, 65, 81))
        };
        Grid.SetColumn(label, 0);
        sectionGrid.Children.Add(label);

        var inputPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        // ✅ KORRIGIERT: TextBox mit korrekten Padding-Parametern
        textBox = new TextBox
        {
            Width = 80,
            TextAlignment = TextAlignment.Center,
            Padding = new Thickness(12, 8, 12, 8),
            FontSize = 14,
            FontWeight = FontWeights.Medium,
            BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            BorderThickness = new Thickness(1),
            ToolTip = tooltip
        };

        var suffix = new TextBlock
        {
            Text = " " + suffixText,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0),
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128))
        };

        inputPanel.Children.Add(textBox);
        inputPanel.Children.Add(suffix);

        Grid.SetColumn(inputPanel, 1);
        sectionGrid.Children.Add(inputPanel);

        sectionBorder.Child = sectionGrid;

        return sectionBorder;
    }

    private Button CreateModernButton(string content, Color startColor, Color endColor)
    {
        var button = new Button
        {
            Content = content,
            Padding = new Thickness(24, 12, 24, 12),
            FontSize = 14,
            FontWeight = FontWeights.Medium,
            Foreground = GetBrush("TextBrush", Brushes.Black),
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            Background = GetBrush("DialogPrimaryGradient", new LinearGradientBrush(startColor, endColor, 90))
        };

        var template = new ControlTemplate(typeof(Button));
        var factory = new FrameworkElementFactory(typeof(Border));
        factory.Name = "border";
        factory.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
        factory.SetValue(Border.PaddingProperty, new System.Windows.Data.Binding("Padding") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        factory.SetValue(Border.EffectProperty, new DropShadowEffect
        {
            Color = Colors.Black,
            Direction = 270,
            ShadowDepth = 2,
            BlurRadius = 8,
            Opacity = 0.1
        });

        var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        factory.AppendChild(contentPresenter);

        template.VisualTree = factory;
        button.Template = template;

        return button;
    }

    // ✅ KORRIGIERT: Event Handler für Buttons
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        // Apply settings without closing
        if (ApplySettings())
        {
            MessageBox.Show(
                _localizationService.GetString("SettingsApplied") ?? "Einstellungen wurden angewendet.",
                _localizationService.GetString("Information") ?? "Information",
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Save and close
        if (ApplySettings())
        {
            DialogResult = true;
            Close();
        }
    }

    private bool ApplySettings()
    {
        if (!int.TryParse(_subTabIntervalTextBox.Text, out int tabInterval) || tabInterval < 1 || tabInterval > 300)
        {
            MessageBox.Show(
                _localizationService.GetString("InvalidTabInterval") ?? 
                "Ungültiger Wert für Tab-Zeit. Bitte geben Sie eine Zahl zwischen 1 und 300 Sekunden ein.",
                _localizationService.GetString("Error") ?? "Fehler",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            _subTabIntervalTextBox.Focus();
            _subTabIntervalTextBox.SelectAll();
            return false;
        }

        // Set both intervals to the same value for simplicity
        ClassInterval = tabInterval;
        SubTabInterval = tabInterval;
        ShowOnlyActiveClasses = _showOnlyActiveCheckBox.IsChecked == true;

        System.Diagnostics.Debug.WriteLine($"⚙️ [OverviewConfig] Settings applied: " +
            $"Interval={tabInterval}s, ShowOnlyActive={ShowOnlyActiveClasses}");

        return true;
    }
}