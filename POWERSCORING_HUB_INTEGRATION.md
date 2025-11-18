# PowerScoring Hub Integration - Implementation Guide

## Übersicht
Diese Datei dokumentiert die Änderungen für die Hub-Integration des PowerScoring-Systems.

## Erforderliche Änderungen

### 1. PowerScoringWindow.xaml.cs - Neue/Geänderte Methoden

```csharp
// Neue Felder hinzufügen
private readonly LicensedHubService? _hubService;
private readonly ConfigService? _configService;
private bool _isRegisteredWithHub = false;

// Konstruktor erweitern
public PowerScoringWindow(
    PowerScoringService powerScoringService, 
    LocalizationService localizationService,
    LicensedHubService? hubService = null,
    ConfigService? configService = null)
{
    InitializeComponent();
    
    _powerScoringService = powerScoringService;
    _localizationService = localizationService;
    _hubService = hubService;
    _configService = configService;

    InitializeRuleComboBox();
    LoadSavedTournamentId();
    UpdateUI();
    
    // Subscribe zu PowerScoring Updates
    _powerScoringService.PlayerScoreUpdated += OnPlayerScoreUpdated;
}

// Neue Methode: GenerateId_Click
private void GenerateId_Click(object sender, RoutedEventArgs e)
{
    try
    {
        if (_hubService != null)
        {
            var newId = _hubService.InnerHubService.GenerateNewTournamentId();
            TournamentIdTextBox.Text = newId;
            _powerScoringService.SetTournamentId(newId);
            
            System.Diagnostics.Debug.WriteLine($"?? Generated new Tournament ID: {newId}");
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error generating ID: {ex.Message}", "Error", 
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

// Neue Methode: LoadSavedTournamentId
private void LoadSavedTournamentId()
{
    try
    {
        // Lade gespeicherte Tournament-ID falls vorhanden
        var tournamentData = _powerScoringService.CurrentSession;
        if (tournamentData != null && !string.IsNullOrWhiteSpace(tournamentData.TournamentId))
        {
            TournamentIdTextBox.Text = tournamentData.TournamentId;
        }
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"?? Could not load saved Tournament ID: {ex.Message}");
    }
}

// Geänderte Methode: StartScoringButton_Click
private async void StartScoringButton_Click(object sender, RoutedEventArgs e)
{
    var session = _powerScoringService.CurrentSession;
    
    if (session == null || session.Players.Count == 0)
    {
        MessageBox.Show("Please add at least one player.", "No Players", 
            MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }

    // Hole/Generiere Tournament-ID
    string? tournamentId = TournamentIdTextBox.Text.Trim();
    if (string.IsNullOrWhiteSpace(tournamentId) && _hubService != null)
    {
        tournamentId = _hubService.InnerHubService.GenerateNewTournamentId();
        TournamentIdTextBox.Text = tournamentId;
    }
    
    _powerScoringService.SetTournamentId(tournamentId);

    // Registriere mit Hub
    if (_hubService != null && !string.IsNullOrEmpty(tournamentId))
    {
        try
        {
            var registered = await _hubService.RegisterTournamentAsync(tournamentId);
            if (registered)
            {
                _isRegisteredWithHub = true;
                _powerScoringService.SetRegisteredWithHub(true);
                
                // Generiere QR-Codes
                var hubUrl = _configService?.Config?.HubUrl ?? "http://localhost:3000";
                _powerScoringService.GenerateQrCodeUrls(hubUrl);
                
                System.Diagnostics.Debug.WriteLine($"? PowerScoring registered with Hub: {tournamentId}");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hub registration failed: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    if (!_powerScoringService.StartScoring())
    {
        MessageBox.Show("Scoring could not be started.", "Error", 
            MessageBoxButton.OK, MessageBoxImage.Error);
        return;
    }

    // UI für Scoring-Phase anpassen
    SetupPanel.Visibility = Visibility.Collapsed;
    ScoringPanel.Visibility = Visibility.Visible;
    StartScoringButton.Visibility = Visibility.Collapsed;
    CompleteScoringButton.Visibility = Visibility.Visible;

    BuildScoringUIWithQrCodes();
    System.Diagnostics.Debug.WriteLine("?? Scoring-Phase gestartet mit QR-Codes");
}

// Neue Methode: BuildScoringUIWithQrCodes
private void BuildScoringUIWithQrCodes()
{
    ScoringItems.Items.Clear();
    var session = _powerScoringService.CurrentSession;
    
    if (session == null) return;

    foreach (var player in session.Players)
    {
        var playerPanel = CreatePlayerQrCodePanel(player);
        ScoringItems.Items.Add(playerPanel);
    }
}

// Neue Methode: CreatePlayerQrCodePanel
private Border CreatePlayerQrCodePanel(PowerScoringPlayer player)
{
    var border = new Border
    {
        Background = (System.Windows.Media.Brush)Application.Current.Resources["BackgroundBrush"],
        BorderBrush = (System.Windows.Media.Brush)Application.Current.Resources["BorderBrush"],
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(6),
        Padding = new Thickness(15),
        Margin = new Thickness(0, 0, 0, 10)
    };

    var mainGrid = new Grid();
    mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
    mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

    // Linke Spalte: Spielername + QR-Code
    var leftStack = new StackPanel();
    
    var nameBlock = new TextBlock
    {
        Text = $"?? {player.Name}",
        FontSize = 16,
        FontWeight = FontWeights.SemiBold,
        Margin = new Thickness(0, 0, 0, 10)
    };
    leftStack.Children.Add(nameBlock);

    // QR-Code (als Placeholder oder echtes Bild)
    if (!string.IsNullOrEmpty(player.QrCodeUrl))
    {
        var qrCodeImage = GenerateQrCodeImage(player.QrCodeUrl);
        if (qrCodeImage != null)
        {
            leftStack.Children.Add(qrCodeImage);
        }
        
        // URL zum Kopieren
        var urlTextBlock = new TextBlock
        {
            Text = player.QrCodeUrl,
            FontSize = 10,
            TextWrapping = TextWrapping.Wrap,
            Foreground = (System.Windows.Media.Brush)Application.Current.Resources["SecondaryTextBrush"],
            Margin = new Thickness(0, 5, 0, 0)
        };
        leftStack.Children.Add(urlTextBlock);
    }

    Grid.SetColumn(leftStack, 0);
    mainGrid.Children.Add(leftStack);

    // Rechte Spalte: Live-Scores
    var rightStack = new StackPanel { Margin = new Thickness(20, 0, 0, 0) };
    
    var scoreTitle = new TextBlock
    {
        Text = "?? Live Score:",
        FontWeight = FontWeights.SemiBold,
        FontSize = 14,
        Margin = new Thickness(0, 0, 0, 10)
    };
    rightStack.Children.Add(scoreTitle);

    // Score Display (wird via Binding aktualisiert)
    var scoreDisplay = new TextBlock
    {
        Text = player.IsScored ? 
            $"Total: {player.TotalScore}\nAverage: {player.AverageScore:F2}" : 
            "Waiting for scores...",
        FontSize = 13,
        Tag = player.PlayerId // Für späteres Update
    };
    rightStack.Children.Add(scoreDisplay);

    Grid.SetColumn(rightStack, 1);
    mainGrid.Children.Add(rightStack);

    border.Child = mainGrid;
    return border;
}

// Neue Methode: GenerateQrCodeImage
private System.Windows.Controls.Image? GenerateQrCodeImage(string url)
{
    try
    {
        // TODO: QR-Code-Bibliothek verwenden (z.B. QRCoder NuGet Package)
        // Placeholder für jetzt
        var image = new System.Windows.Controls.Image
        {
            Width = 200,
            Height = 200,
            Margin = new Thickness(0, 10, 0, 10)
        };
        
        // Erstelle QR-Code (Placeholder - echte Implementierung benötigt QRCoder)
        // var qrGenerator = new QRCodeGenerator();
        // var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        // var qrCode = new QRCode(qrCodeData);
        // var qrCodeBitmap = qrCode.GetGraphic(20);
        
        return image;
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error generating QR code: {ex.Message}");
        return null;
    }
}

// Neue Methode: OnPlayerScoreUpdated
private void OnPlayerScoreUpdated(object? sender, PowerScoringPlayer player)
{
    Dispatcher.Invoke(() =>
    {
        try
        {
            // Finde das UI-Element für diesen Spieler und aktualisiere es
            foreach (var item in ScoringItems.Items)
            {
                if (item is Border border && border.Child is Grid grid)
                {
                    // Finde Score-Display anhand Tag
                    foreach (var child in FindVisualChildren<TextBlock>(grid))
                    {
                        if (child.Tag is Guid playerId && playerId == player.PlayerId)
                        {
                            child.Text = $"Total: {player.TotalScore}\nAverage: {player.AverageScore:F2}";
                            break;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating player score UI: {ex.Message}");
        }
    });
}

// Helper Methode: FindVisualChildren
private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
{
    if (depObj != null)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
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

// Window Closing - Hub-Unregistrierung
protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
{
    if (_isRegisteredWithHub && _hubService != null)
    {
        try
        {
            _ = _hubService.UnregisterTournamentAsync();
        }
        catch { }
    }
    
    base.OnClosing(e);
}
```

### 2. WebSocket Message Handler Erweiterung

In `WebSocketMessageHandler.cs` muss PowerScoring Message-Handling hinzugefügt werden:

```csharp
// In ProcessMessage Methode
if (type == "power-scoring-update" || type == "power-scoring-result")
{
    await HandlePowerScoringMessage(message);
    return;
}

private async Task HandlePowerScoringMessage(JsonElement message)
{
    try
    {
        var powerScoringMessage = new PowerScoringHubMessage
        {
            Type = message.GetProperty("type").GetString() ?? "",
            TournamentId = message.GetProperty("tournamentId").GetString() ?? "",
            ParticipantId = message.GetProperty("participantId").GetString() ?? "",
            PlayerName = message.GetProperty("playerName").GetString() ?? "",
            Rounds = message.GetProperty("rounds").GetInt32(),
            Throws = message.GetProperty("throws").GetInt32(),
            TotalScore = message.GetProperty("totalScore").GetInt32(),
            Average = message.GetProperty("average").GetDouble(),
            Timestamp = message.TryGetProperty("timestamp", out var ts) ? 
                DateTime.Parse(ts.GetString() ?? "") : null
        };

        // Parse History
        if (message.TryGetProperty("history", out var historyArray))
        {
            powerScoringMessage.History = new List<PowerScoringHistoryItem>();
            foreach (var item in historyArray.EnumerateArray())
            {
                powerScoringMessage.History.Add(new PowerScoringHistoryItem
                {
                    Round = item.GetProperty("round").GetInt32(),
                    Throw1 = item.GetProperty("throw1").GetInt32(),
                    Throw2 = item.GetProperty("throw2").GetInt32(),
                    Throw3 = item.GetProperty("throw3").GetInt32(),
                    Total = item.GetProperty("total").GetInt32()
                });
            }
        }

        // Verarbeite über PowerScoringService
        App.Current?.Dispatcher.Invoke(() =>
        {
            // PowerScoringService muss global zugänglich sein oder über Event-System
            OnPowerScoringMessageReceived?.Invoke(this, powerScoringMessage);
        });
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"? Error processing PowerScoring message: {ex.Message}");
    }
}

// Event für PowerScoring Messages
public event EventHandler<PowerScoringHubMessage>? OnPowerScoringMessageReceived;
```

### 3. MainWindow.xaml.cs Integration

```csharp
// In PowerScoring_Click Methode - übergebe Hub-Service
private void PowerScoring_Click(object sender, RoutedEventArgs e)
{
    try
    {
        // ... Lizenzprüfung ...

        // Öffne PowerScoring-Fenster mit Hub-Service
        var powerScoringWindow = new PowerScoringWindow(
            _powerScoringService, 
            _localizationService,
            _hubService,  // NEU: Übergebe Hub-Service
            _configService); // NEU: Übergebe Config-Service
        powerScoringWindow.Owner = this;
        powerScoringWindow.ShowDialog();
        
        System.Diagnostics.Debug.WriteLine("? PowerScoring-Fenster geöffnet");
    }
    catch (Exception ex)
    {
        // Error handling...
    }
}
```

### 4. Required NuGet Package

Für QR-Code-Generierung:
```xml
<PackageReference Include="QRCoder" Version="1.4.3" />
```

### 5. Nächste Schritte

1. QRCoder NuGet Package installieren
2. `GenerateQrCodeImage` Methode implementieren
3. WebSocket Message Handler erweitern
4. PowerScoring Service global zugänglich machen oder Event-System nutzen
5. UI-Tests durchführen

## Wichtige Hinweise

- Die QR-Codes werden nach Hub-Registrierung generiert
- WebSocket-Updates aktualisieren die UI in Echtzeit
- Tournament-ID wird zwischen HubRegistrationDialog und PowerScoringWindow synchronisiert
- Manuelle Score-Eingabe wurde durch QR-Code + Hub-System ersetzt
