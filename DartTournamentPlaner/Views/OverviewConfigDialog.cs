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

    public OverviewConfigDialog()
    {
        _localizationService = App.LocalizationService ?? throw new InvalidOperationException("LocalizationService not available");
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Title = _localizationService.GetString("OverviewConfiguration");
        Width = 500;
        Height = 400;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        
        // Moderner Glaseffekt-Hintergrund
        Background = new LinearGradientBrush(
            Color.FromRgb(248, 250, 252),
            Color.FromRgb(241, 245, 249), 90);

        // Hauptcontainer mit modernem Card-Design
        var mainBorder = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromRgb(255, 255, 255),
                Color.FromRgb(253, 253, 253), 90),
            CornerRadius = new CornerRadius(16),
            Margin = new Thickness(16, 16, 16, 16),
            Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 270,
                ShadowDepth = 8,
                BlurRadius = 24,
                Opacity = 0.12
            }
        };

        var grid = new Grid { Margin = new Thickness(32, 32, 32, 32) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(16) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Header mit Icon and Gradient
        var headerBorder = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromRgb(30, 64, 175),
                Color.FromRgb(30, 58, 138), 90),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(20, 16, 20, 16),
            Margin = new Thickness(0, 0, 0, 24),
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
            Text = _localizationService.GetString("TournamentOverviewConfiguration"),
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };

        headerPanel.Children.Add(iconText);
        headerPanel.Children.Add(titleBlock);
        headerBorder.Child = headerPanel;

        Grid.SetRow(headerBorder, 0);
        Grid.SetColumnSpan(headerBorder, 2);
        grid.Children.Add(headerBorder);

        // Class Interval Section
        var classSection = CreateInputSection(
            _localizationService.GetString("TimeBetweenClasses"),
            out _classIntervalTextBox,
            _localizationService.GetString("Seconds"),
            "Zeit zwischen verschiedenen Turnierklassen"
        );
        Grid.SetRow(classSection, 2);
        Grid.SetColumnSpan(classSection, 2);
        grid.Children.Add(classSection);

        // Sub-Tab Interval Section  
        var subSection = CreateInputSection(
            _localizationService.GetString("TimeBetweenSubTabs"),
            out _subTabIntervalTextBox, 
            _localizationService.GetString("Seconds"),
            "Zeit zwischen Unterreitern in derselben Klasse"
        );
        Grid.SetRow(subSection, 3);
        Grid.SetColumnSpan(subSection, 2);
        grid.Children.Add(subSection);

        // Show Only Active Classes
        var checkboxBorder = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromRgb(239, 246, 255),
                Color.FromRgb(219, 234, 254), 90),
            BorderBrush = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 12, 16, 12),
            Margin = new Thickness(0, 16, 0, 0)
        };

        _showOnlyActiveCheckBox = new CheckBox
        {
            Content = _localizationService.GetString("ShowOnlyActiveClassesText"),
            FontSize = 14,
            FontWeight = FontWeights.Medium,
            Foreground = new SolidColorBrush(Color.FromRgb(30, 64, 175))
        };

        checkboxBorder.Child = _showOnlyActiveCheckBox;
        Grid.SetRow(checkboxBorder, 4);
        Grid.SetColumnSpan(checkboxBorder, 2);
        grid.Children.Add(checkboxBorder);

        // Info Text
        var infoText = new TextBlock
        {
            Text = _localizationService.GetString("OverviewInfoText"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 16, 0, 24),
            Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)),
            FontStyle = FontStyles.Italic,
            FontSize = 13,
            LineHeight = 18
        };
        Grid.SetRow(infoText, 6);
        Grid.SetColumnSpan(infoText, 2);
        grid.Children.Add(infoText);

        // Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 16, 0, 0)
        };

        var cancelButton = CreateModernButton(
            _localizationService.GetString("Cancel"),
            Color.FromRgb(100, 116, 139),
            Color.FromRgb(71, 85, 105)
        );
        cancelButton.IsCancel = true;
        cancelButton.Margin = new Thickness(0, 0, 12, 0);
        cancelButton.Click += (s, e) => { DialogResult = false; Close(); };

        var okButton = CreateModernButton(
            _localizationService.GetString("OK"),
            Color.FromRgb(59, 130, 246),
            Color.FromRgb(37, 99, 235)
        );
        okButton.IsDefault = true;
        okButton.Click += (s, e) => { SaveAndClose(); };

        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(okButton);

        Grid.SetRow(buttonPanel, 7);
        Grid.SetColumnSpan(buttonPanel, 2);
        grid.Children.Add(buttonPanel);

        mainBorder.Child = grid;
        Content = mainBorder;

        Loaded += (s, e) =>
        {
            _classIntervalTextBox.Text = ClassInterval.ToString();
            _subTabIntervalTextBox.Text = SubTabInterval.ToString();
            _showOnlyActiveCheckBox.IsChecked = ShowOnlyActiveClasses;

            _classIntervalTextBox.Focus();
            _classIntervalTextBox.SelectAll();
        };
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
            Margin = new Thickness(0, 8, 0, 8),
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
        sectionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
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
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            Background = new LinearGradientBrush(startColor, endColor, 90)
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

    private void SaveAndClose()
    {
        if (!int.TryParse(_classIntervalTextBox.Text, out int classInterval) || classInterval < 1)
        {
            MessageBox.Show(_localizationService.GetString("InvalidClassInterval"), _localizationService.GetString("Error"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
            _classIntervalTextBox.Focus();
            _classIntervalTextBox.SelectAll();
            return;
        }

        if (!int.TryParse(_subTabIntervalTextBox.Text, out int subTabInterval) || subTabInterval < 1)
        {
            MessageBox.Show(_localizationService.GetString("InvalidSubTabInterval"), _localizationService.GetString("Error"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
            _subTabIntervalTextBox.Focus();
            _subTabIntervalTextBox.SelectAll();
            return;
        }

        ClassInterval = classInterval;
        SubTabInterval = subTabInterval;
        ShowOnlyActiveClasses = _showOnlyActiveCheckBox.IsChecked == true;

        DialogResult = true;
        Close();
    }
}