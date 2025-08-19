using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Controls;

namespace DartTournamentPlaner.Views;

/// <summary>
/// Dialog für die Anzeige verfügbarer Updates
/// Zeigt Changelog und ermöglicht Download oder Überspringen der neuen Version
/// Unterstützt automatischen Download und Installation der Setup.exe
/// </summary>
public partial class UpdateDialog : Window
{
    private readonly UpdateInfo _updateInfo;
    private readonly LocalizationService? _localizationService;
    private readonly UpdateService? _updateService;
    private bool _downloadRequested = false;
    private bool _isDownloading = false;

    public bool DownloadRequested => _downloadRequested;
    public bool InstallationStarted { get; private set; } = false;

    public UpdateDialog(UpdateInfo updateInfo, LocalizationService? localizationService = null, UpdateService? updateService = null)
    {
        _updateInfo = updateInfo ?? throw new ArgumentNullException(nameof(updateInfo));
        _localizationService = localizationService;
        _updateService = updateService;
        
        InitializeComponent();
        InitializeDialog();
    }

    private void InitializeComponent()
    {
        // Window-Eigenschaften
        Title = _localizationService?.GetTranslation("UpdateAvailable") ?? "Update verfügbar";
        Width = 600;
        Height = 650;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        ResizeMode = ResizeMode.NoResize;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ShowInTaskbar = false;
        Topmost = true;

        // Hauptcontainer mit modernem Design
        var mainBorder = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromRgb(248, 249, 250),
                Color.FromRgb(241, 243, 245),
                90),
            CornerRadius = new CornerRadius(16),
            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderThickness = new Thickness(1),
            Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 270,
                ShadowDepth = 12,
                BlurRadius = 30,
                Opacity = 0.2
            },
            Margin = new Thickness(15)
        };

        var mainGrid = new Grid();
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons

        // Header mit Gradient
        var headerBorder = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromRgb(34, 197, 94),   // Grün links
                Color.FromRgb(22, 163, 74),   // Dunkelgrün rechts
                0),
            CornerRadius = new CornerRadius(16, 16, 0, 0),
            Padding = new Thickness(24, 20, 24, 20)
        };

        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Update-Icon
        var updateIcon = new TextBlock
        {
            Text = "🔄",
            FontSize = 24,
            Margin = new Thickness(0, 0, 12, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        // Header-Text
        var headerStack = new StackPanel();
        
        var titleText = new TextBlock
        {
            Text = _localizationService?.GetTranslation("UpdateAvailable") ?? "Update verfügbar",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White
        };

        var versionText = new TextBlock
        {
            Text = $"{_updateInfo.CurrentVersion} → {_updateInfo.LatestVersion}",
            FontSize = 14,
            FontWeight = FontWeights.Medium,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            Margin = new Thickness(0, 4, 0, 0)
        };

        headerStack.Children.Add(titleText);
        headerStack.Children.Add(versionText);
        
        headerPanel.Children.Add(updateIcon);
        headerPanel.Children.Add(headerStack);
        headerBorder.Child = headerPanel;
        Grid.SetRow(headerBorder, 0);

        // Content-Bereich
        var contentPanel = new Grid
        {
            Margin = new Thickness(24, 24, 24, 20)
        };

        contentPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Release Info
        contentPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Changelog

        // Release-Informationen
        var releaseInfoPanel = new StackPanel
        {
            Margin = new Thickness(0, 0, 0, 16)
        };

        // Release-Name und Datum
        var releaseNameText = new TextBlock
        {
            Text = _updateInfo.ReleaseName,
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
            Margin = new Thickness(0, 0, 0, 8)
        };

        var releaseDateText = new TextBlock
        {
            Text = $"Veröffentlicht am: {_updateInfo.PublishedAt:dd.MM.yyyy}",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
            Margin = new Thickness(0, 0, 0, 4)
        };

        // Prerelease-Hinweis falls zutreffend
        if (_updateInfo.IsPrerelease)
        {
            var prereleaseWarning = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(30, 245, 158, 11)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(245, 158, 11)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 8, 0, 0)
            };

            var prereleaseText = new TextBlock
            {
                Text = "⚠️ Dies ist eine Vorabversion (Beta). Es können Instabilitäten auftreten.",
                FontSize = 11,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(146, 64, 14)),
                TextWrapping = TextWrapping.Wrap
            };

            prereleaseWarning.Child = prereleaseText;
            releaseInfoPanel.Children.Add(prereleaseWarning);
        }

        releaseInfoPanel.Children.Add(releaseNameText);
        releaseInfoPanel.Children.Add(releaseDateText);
        Grid.SetRow(releaseInfoPanel, 0);

        // Changelog-Bereich
        var changelogLabel = new TextBlock
        {
            Text = _localizationService?.GetTranslation("WhatsNew") ?? "Was ist neu:",
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
            Margin = new Thickness(0, 16, 0, 12)
        };

        var changelogBorder = new Border
        {
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(2),
            Margin = new Thickness(0, 0, 0, 16)
        };

        var changelogScrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Padding = new Thickness(16, 12, 16, 12),
            MaxHeight = 200
        };

        var changelogText = new TextBlock
        {
            Text = _updateInfo.GetFormattedReleaseNotes(),
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 18,
            FontFamily = new FontFamily("Segoe UI")
        };

        changelogScrollViewer.Content = changelogText;
        changelogBorder.Child = changelogScrollViewer;

        var changelogPanel = new StackPanel();
        changelogPanel.Children.Add(changelogLabel);
        changelogPanel.Children.Add(changelogBorder);
        Grid.SetRow(changelogPanel, 1);

        contentPanel.Children.Add(releaseInfoPanel);
        contentPanel.Children.Add(changelogPanel);
        Grid.SetRow(contentPanel, 1);

        // Button-Bereich
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(24, 0, 24, 24)
        };

        // Später erinnern Button
        var remindLaterButton = new Button
        {
            Content = _localizationService?.GetTranslation("RemindLater") ?? "Später erinnern",
            Width = 130,
            Height = 40,
            Margin = new Thickness(0, 0, 12, 0),
            Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
            FontWeight = FontWeights.Medium,
            FontSize = 13,
            Cursor = Cursors.Hand
        };

        remindLaterButton.Style = CreateModernButtonStyle(false);
        remindLaterButton.Click += (s, e) => { _downloadRequested = false; Close(); };

        // Überspringen Button
        var skipButton = new Button
        {
            Content = _localizationService?.GetTranslation("SkipVersion") ?? "Version überspringen",
            Width = 140,
            Height = 40,
            Margin = new Thickness(0, 0, 12, 0),
            Background = new SolidColorBrush(Color.FromRgb(239, 68, 68)),
            BorderThickness = new Thickness(0),
            Foreground = Brushes.White,
            FontWeight = FontWeights.Medium,
            FontSize = 13,
            Cursor = Cursors.Hand
        };

        skipButton.Style = CreateModernButtonStyle(true, Color.FromRgb(220, 38, 38));
        skipButton.Click += (s, e) => { 
            _downloadRequested = false; 
            // TODO: Implementiere "Skip Version" Logik
            Close(); 
        };

        // Download Button
        var downloadButton = new Button
        {
            Content = _updateInfo.CanAutoDownload ? 
                $"💾 {(_localizationService?.GetTranslation("DownloadAndInstall") ?? "Herunterladen & Installieren")}" :
                $"📥 {(_localizationService?.GetTranslation("DownloadUpdate") ?? "Jetzt herunterladen")}",
            Width = _updateInfo.CanAutoDownload ? 200 : 160,
            Height = 40,
            Background = new LinearGradientBrush(
                Color.FromRgb(34, 197, 94),
                Color.FromRgb(22, 163, 74),
                90),
            BorderThickness = new Thickness(0),
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold,
            FontSize = 13,
            Cursor = Cursors.Hand,
            IsDefault = true
        };

        downloadButton.Style = CreateModernButtonStyle(true, Color.FromRgb(22, 163, 74));
        downloadButton.Click += DownloadButton_Click;

        buttonPanel.Children.Add(remindLaterButton);
        buttonPanel.Children.Add(skipButton);
        buttonPanel.Children.Add(downloadButton);
        Grid.SetRow(buttonPanel, 2);

        mainGrid.Children.Add(headerBorder);
        mainGrid.Children.Add(contentPanel);
        mainGrid.Children.Add(buttonPanel);
        mainBorder.Child = mainGrid;

        Content = mainBorder;

        // Event-Handler
        KeyDown += (s, e) =>
        {
            if (e.Key == Key.Escape && !_isDownloading)
            {
                _downloadRequested = false;
                Close();
            }
            else if (e.Key == Key.Enter && !_isDownloading)
            {
                DownloadButton_Click(downloadButton, new RoutedEventArgs());
            }
        };

        // Window bewegbar machen
        headerBorder.MouseLeftButtonDown += (s, e) => { if (!_isDownloading) DragMove(); };

        // Focus auf Download-Button
        Loaded += (s, e) => downloadButton.Focus();
    }

    /// <summary>
    /// Event-Handler für Download-Button
    /// </summary>
    private async void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isDownloading)
            return;

        _downloadRequested = true;

        if (_updateInfo.CanAutoDownload && _updateService != null)
        {
            await StartAutomaticDownloadAsync();
        }
        else
        {
            // Fallback: Öffne Browser
            OpenDownloadPage();
            Close();
        }
    }

    /// <summary>
    /// Startet den automatischen Download mit Progress-Anzeige
    /// </summary>
    private async Task StartAutomaticDownloadAsync()
    {
        if (_updateService == null)
        {
            OpenDownloadPage();
            Close();
            return;
        }

        try
        {
            _isDownloading = true;
            
            // UI für Download-Modus vorbereiten
            await ShowDownloadProgressAsync();
            
            var progress = new Progress<(string Status, int Percentage)>(update =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    UpdateDownloadProgress(update.Status, update.Percentage);
                });
            });

            System.Diagnostics.Debug.WriteLine("UpdateDialog: Starting automatic download...");
            
            // Download starten
            var success = await _updateService.DownloadAndInstallUpdateAsync(_updateInfo, progress);
            
            if (success)
            {
                System.Diagnostics.Debug.WriteLine("UpdateDialog: Download and installation started successfully");
                InstallationStarted = true;
                
                Dispatcher.BeginInvoke(() =>
                {
                    UpdateDownloadProgress("Installation gestartet - Anwendung wird beendet", 100);
                });
                
                // Kurz warten damit User die Nachricht sieht
                await Task.Delay(2000);
                
                // Dialog schließen - App wird von App.xaml.cs beendet
                Close();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("UpdateDialog: Download failed, falling back to browser");
                
                Dispatcher.BeginInvoke(() =>
                {
                    UpdateDownloadProgress("Download fehlgeschlagen - Browser wird geöffnet", -1);
                });
                
                await Task.Delay(2000);
                
                // Fallback: Öffne Browser
                OpenDownloadPage();
                Close();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateDialog: Download error: {ex.Message}");
            
            Dispatcher.BeginInvoke(() =>
            {
                UpdateDownloadProgress($"Fehler: {ex.Message}", -1);
            });
            
            await Task.Delay(3000);
            
            // Fallback: Öffne Browser
            OpenDownloadPage();
            Close();
        }
        finally
        {
            _isDownloading = false;
        }
    }

    /// <summary>
    /// Zeigt Download-Progress UI an
    /// </summary>
    private async Task ShowDownloadProgressAsync()
    {
        await Dispatcher.InvokeAsync(() =>
        {
            // Dialog auf Download-Modus umstellen
            Title = _localizationService?.GetTranslation("DownloadingUpdate") ?? "Update wird heruntergeladen";
            
            // Buttons deaktivieren
            foreach (Button button in FindVisualChildren<Button>(this))
            {
                button.IsEnabled = false;
            }
            
            // Progress-Anzeige hinzufügen (vereinfacht - in einer echten Implementierung würde man das Layout anpassen)
            System.Diagnostics.Debug.WriteLine("UpdateDialog: UI switched to download mode");
        });
    }

    /// <summary>
    /// Aktualisiert den Download-Progress
    /// </summary>
    private void UpdateDownloadProgress(string status, int percentage)
    {
        System.Diagnostics.Debug.WriteLine($"UpdateDialog: Download progress: {status} ({percentage}%)");
        
        // Hier könnte man eine Progress-Bar oder Status-Text aktualisieren
        // Für diese Implementierung loggen wir nur den Fortschritt
    }

    /// <summary>
    /// Hilfsmethode um alle Child-Controls eines bestimmten Typs zu finden
    /// </summary>
    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj != null)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T)
                {
                    yield return (T)child;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }
    }

    private void InitializeDialog()
    {
        System.Diagnostics.Debug.WriteLine($"UpdateDialog: Initialized for version {_updateInfo.LatestVersion}");
    }

    /// <summary>
    /// Öffnet die Download-Seite im Standard-Browser
    /// </summary>
    public void OpenDownloadPage()
    {
        try
        {
            if (!string.IsNullOrEmpty(_updateInfo.DownloadUrl))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _updateInfo.DownloadUrl,
                    UseShellExecute = true
                };
                Process.Start(psi);
                
                System.Diagnostics.Debug.WriteLine($"UpdateDialog: Opened download page: {_updateInfo.DownloadUrl}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateDialog: Error opening download page: {ex.Message}");
            
            // Fallback: Zeige URL in einer Nachricht
            MessageBox.Show(
                $"Bitte besuchen Sie die folgende URL um das Update herunterzuladen:\n\n{_updateInfo.DownloadUrl}",
                "Download-Link",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    /// <summary>
    /// Erstellt einen modernen Button-Style
    /// </summary>
    private Style CreateModernButtonStyle(bool isPrimary, Color? hoverColor = null)
    {
        var style = new Style(typeof(Button));

        var template = new ControlTemplate(typeof(Button));
        
        var border = new FrameworkElementFactory(typeof(Border));
        border.Name = "border";
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
        border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") 
        { 
            RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) 
        });
        border.SetBinding(Border.BorderBrushProperty, new System.Windows.Data.Binding("BorderBrush") 
        { 
            RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) 
        });
        border.SetBinding(Border.BorderThicknessProperty, new System.Windows.Data.Binding("BorderThickness") 
        { 
            RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) 
        });
        
        var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        contentPresenter.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        contentPresenter.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        
        border.AppendChild(contentPresenter);
        template.VisualTree = border;

        // Hover-Effekt
        var hoverTrigger = new System.Windows.Trigger
        {
            Property = UIElement.IsMouseOverProperty,
            Value = true
        };

        if (isPrimary)
        {
            var color = hoverColor ?? Color.FromRgb(37, 99, 235);
            hoverTrigger.Setters.Add(new Setter
            {
                Property = Control.BackgroundProperty,
                Value = new SolidColorBrush(color)
            });
        }
        else
        {
            hoverTrigger.Setters.Add(new Setter
            {
                Property = Control.BackgroundProperty,
                Value = new SolidColorBrush(Color.FromRgb(241, 245, 249))
            });
        }

        // Schatten-Effekt beim Hover
        hoverTrigger.Setters.Add(new Setter
        {
            Property = Control.EffectProperty,
            Value = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 270,
                ShadowDepth = 4,
                BlurRadius = 12,
                Opacity = 0.15
            }
        });

        template.Triggers.Add(hoverTrigger);
        style.Setters.Add(new Setter(Control.TemplateProperty, template));

        return style;
    }
}