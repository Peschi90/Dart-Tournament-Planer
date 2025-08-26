using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace DartTournamentPlaner.Views;

public partial class HubDebugWindow : Window
{
    private int _messageCount = 0;
    private readonly object _lockObject = new();
    
    // UI Elements - fallback wenn XAML nicht lädt
    private TextBox? DebugTextBox;
    private ScrollViewer? DebugScrollViewer;
    private Ellipse? ConnectionIndicator;
    private TextBlock? ConnectionStatus;
    private TextBlock? StatusText;
    private TextBlock? MessageCountText;
    private CheckBox? AutoScrollCheckBox;

    public HubDebugWindow()
    {
        // Immer Fallback-UI verwenden
        CreateFallbackUI();
        InitializeDebugWindow();
    }

    private void CreateFallbackUI()
    {
        Title = "Tournament Hub Debug Console";
        Width = 1000;
        Height = 600;
        Background = new SolidColorBrush(Color.FromRgb(26, 26, 26));
        Foreground = Brushes.White;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Header
        var headerBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
            Padding = new Thickness(15),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Tournament Hub Debug Console",
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = Brushes.White
                    },
                    (ConnectionIndicator = new Ellipse
                    {
                        Width = 12,
                        Height = 12,
                        Fill = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                        Margin = new Thickness(15, 0, 5, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    }),
                    (ConnectionStatus = new TextBlock
                    {
                        Text = "Getrennt",
                        FontSize = 12,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = Brushes.White
                    })
                }
            }
        };
        Grid.SetRow(headerBorder, 0);
        grid.Children.Add(headerBorder);

        // Main content
        DebugScrollViewer = new ScrollViewer
        {
            Background = new SolidColorBrush(Color.FromRgb(26, 26, 26)),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        DebugTextBox = new TextBox
        {
            IsReadOnly = true,
            TextWrapping = TextWrapping.Wrap,
            FontFamily = new FontFamily("Consolas"),
            FontSize = 12,
            Padding = new Thickness(10),
            BorderThickness = new Thickness(0),
            Background = new SolidColorBrush(Color.FromRgb(42, 42, 42)),
            Foreground = Brushes.White,
            AcceptsReturn = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        DebugScrollViewer.Content = DebugTextBox;
        Grid.SetRow(DebugScrollViewer, 1);
        grid.Children.Add(DebugScrollViewer);

        // Status bar
        var statusBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(42, 42, 42)),
            Padding = new Thickness(10, 5, 10, 5)
        };

        var statusGrid = new Grid();
        statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        StatusText = new TextBlock
        {
            Text = "Ready for debugging...",
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Brushes.White
        };
        Grid.SetColumn(StatusText, 0);
        statusGrid.Children.Add(StatusText);

        MessageCountText = new TextBlock
        {
            Text = "Messages: 0",
            FontSize = 11,
            Margin = new Thickness(20, 0, 20, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Brushes.White
        };
        Grid.SetColumn(MessageCountText, 1);
        statusGrid.Children.Add(MessageCountText);

        AutoScrollCheckBox = new CheckBox
        {
            Content = "Auto-Scroll",
            IsChecked = true,
            Foreground = Brushes.White,
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(AutoScrollCheckBox, 2);
        statusGrid.Children.Add(AutoScrollCheckBox);

        statusBorder.Child = statusGrid;
        Grid.SetRow(statusBorder, 2);
        grid.Children.Add(statusBorder);

        Content = grid;
    }

    private void InitializeDebugWindow()
    {
        try
        {
            // Initial setup
            if (DebugTextBox != null)
            {
                DebugTextBox.Text = "Tournament Hub Debug Console gestartet\n";
                DebugTextBox.Text += $"Zeit: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
                DebugTextBox.Text += "=====================================\n\n";
                
                // Auto-scroll to bottom
                DebugTextBox.TextChanged += (s, e) => 
                {
                    if (AutoScrollCheckBox?.IsChecked == true)
                    {
                        DebugScrollViewer?.ScrollToBottom();
                    }
                };
            }
            
            UpdateStatus("Warte auf Hub-Verbindung...");
            UpdateMessageCount();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing debug window: {ex.Message}");
        }
    }

    /// <summary>
    /// Fügt eine Debug-Nachricht hinzu
    /// </summary>
    public void AddDebugMessage(string message, string category = "INFO")
    {
        Dispatcher.Invoke(() =>
        {
            lock (_lockObject)
            {
                try
                {
                    if (DebugTextBox == null) return;
                    
                    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    var colorCode = GetColorForCategory(category);
                    var formattedMessage = $"[{timestamp}] {colorCode} {message}\n";
                    
                    DebugTextBox.AppendText(formattedMessage);
                    _messageCount++;
                    
                    UpdateMessageCount();
                    
                    // Auto-scroll if enabled
                    if (AutoScrollCheckBox?.IsChecked == true)
                    {
                        DebugScrollViewer?.ScrollToBottom();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error adding debug message: {ex.Message}");
                }
            }
        });
    }

    /// <summary>
    /// Aktualisiert den Verbindungsstatus
    /// </summary>
    public void UpdateConnectionStatus(bool isConnected, string status)
    {
        Dispatcher.Invoke(() =>
        {
            try
            {
                if (ConnectionIndicator != null && ConnectionStatus != null)
                {
                    if (isConnected)
                    {
                        ConnectionIndicator.Fill = new SolidColorBrush(Color.FromRgb(46, 204, 113));
                        ConnectionStatus.Text = "Verbunden";
                        UpdateStatus($"Hub verbunden: {status}");
                        AddDebugMessage($"Hub-Verbindung hergestellt: {status}", "SUCCESS");
                    }
                    else
                    {
                        ConnectionIndicator.Fill = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                        ConnectionStatus.Text = "Getrennt";
                        UpdateStatus($"Hub getrennt: {status}");
                        AddDebugMessage($"Hub-Verbindung getrennt: {status}", "ERROR");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating connection status: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// Hilfsmethode: Farbcode für Kategorie
    /// </summary>
    private string GetColorForCategory(string category)
    {
        return category.ToUpper() switch
        {
            "SUCCESS" => "[✅ OK]",
            "ERROR" => "[❌ ERR]",
            "WARNING" => "[⚠️ WARN]",
            "WEBSOCKET" => "[🔌 WS]",
            "MATCH" => "[🎯 MATCH]",
            "MATCH_RESULT" => "[🏆 RESULT]", // Spezielle Kennzeichnung für Match-Ergebnisse
            "TOURNAMENT" => "[🏆 TOURN]",
            "SYNC" => "[🔄 SYNC]",
            _ => "[ℹ️ INFO]"
        };
    }

    /// <summary>
    /// Aktualisiert den Status-Text
    /// </summary>
    public void UpdateStatus(string status)
    {
        try
        {
            if (StatusText != null)
            {
                StatusText.Text = status;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating status: {ex.Message}");
        }
    }

    /// <summary>
    /// Aktualisiert die Message-Anzahl
    /// </summary>
    private void UpdateMessageCount()
    {
        try
        {
            if (MessageCountText != null)
            {
                MessageCountText.Text = $"Messages: {_messageCount}";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating message count: {ex.Message}");
        }
    }

    /// <summary>
    /// Clear Button Click
    /// </summary>
    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = MessageBox.Show(
                "Möchten Sie alle Debug-Nachrichten löschen?", 
                "Debug Console löschen", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                lock (_lockObject)
                {
                    DebugTextBox?.Clear();
                    _messageCount = 0;
                    UpdateMessageCount();
                    UpdateStatus("Debug Console geleert");
                    
                    // Header wieder hinzufügen
                    if (DebugTextBox != null)
                    {
                        DebugTextBox.Text = "Tournament Hub Debug Console\n";
                        DebugTextBox.Text += $"Zeit: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
                        DebugTextBox.Text += "=====================================\n\n";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Löschen: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Save Button Click
    /// </summary>
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|Log files (*.log)|*.log|All files (*.*)|*.*",
                DefaultExt = "txt",
                FileName = $"HubDebug_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (saveDialog.ShowDialog() == true && DebugTextBox != null)
            {
                File.WriteAllText(saveDialog.FileName, DebugTextBox.Text);
                UpdateStatus($"Debug Log gespeichert: {saveDialog.FileName}");
                AddDebugMessage($"Debug Log gespeichert unter: {saveDialog.FileName}", "SUCCESS");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Speichern: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Close Button Click
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide(); // Verstecken statt schließen, damit das Fenster wiederverwendet werden kann
    }

    /// <summary> 
    /// Window Closing Event
    /// </summary>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true; // Cancel closing
        Hide(); // Hide instead of close
    }
}