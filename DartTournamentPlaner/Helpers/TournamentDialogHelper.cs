using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Input;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Helper-Klasse für Dialog-Funktionen im TournamentTab
/// Enthält Methoden zur Erstellung und Anzeige von Dialogen
/// </summary>
public static class TournamentDialogHelper
{
    /// <summary>
    /// Zeigt einen modernen Eingabedialog an mit ansprechendem Design
    /// </summary>
    /// <param name="owner">Das übergeordnete Fenster</param>
    /// <param name="prompt">Die Eingabeaufforderung</param>
    /// <param name="title">Der Titel des Dialogs</param>
    /// <param name="defaultValue">Der Standardwert</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>Die eingegebene Zeichenkette oder null wenn abgebrochen</returns>
    public static string? ShowInputDialog(Window? owner, string prompt, string title, string defaultValue = "", LocalizationService? localizationService = null)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 450,
            Height = 260,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = System.Windows.Media.Brushes.Transparent,
            ShowInTaskbar = false
        };

        if (owner != null)
        {
            dialog.Owner = owner;
        }

        // Hauptcontainer mit abgerundeten Ecken und Schatten
        var mainBorder = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromRgb(248, 249, 250),  // Helles Grau oben
                Color.FromRgb(241, 243, 245),  // Etwas dunkleres Grau unten
                90),
            CornerRadius = new CornerRadius(12),
            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderThickness = new Thickness(1),
            Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 270,
                ShadowDepth = 8,
                BlurRadius = 20,
                Opacity = 0.15
            },
            Margin = new Thickness(10)
        };

        var mainGrid = new Grid
        {
            Margin = new Thickness(0)
        };
        
        // Definiere Reihen
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons

        // Header mit Icon und Titel
        var headerBorder = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromRgb(59, 130, 246),   // Blau links
                Color.FromRgb(99, 102, 241),   // Lila rechts
                0),
            CornerRadius = new CornerRadius(12, 12, 0, 0),
            Padding = new Thickness(24, 16, 24, 16)
        };

        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Icon
        var iconText = new TextBlock
        {
            Text = "📝",
            FontSize = 20,
            Margin = new Thickness(0, 0, 12, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        // Titel
        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };

        headerPanel.Children.Add(iconText);
        headerPanel.Children.Add(titleText);
        headerBorder.Child = headerPanel;
        Grid.SetRow(headerBorder, 0);

        // Content-Bereich
        var contentPanel = new StackPanel
        {
            Margin = new Thickness(24, 24, 24, 20),
            VerticalAlignment = VerticalAlignment.Center
        };

        // Prompt-Text
        var promptText = new TextBlock
        {
            Text = prompt,
            FontSize = 14,
            FontWeight = FontWeights.Medium,
            Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
            Margin = new Thickness(0, 0, 0, 16),
            TextWrapping = TextWrapping.Wrap
        };

        // Eingabefeld mit modernem Design
        var textBoxBorder = new Border
        {
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(2)
        };

        var textBox = new TextBox
        {
            Text = defaultValue,
            FontSize = 14,
            Padding = new Thickness(12, 10, 12, 10),
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            VerticalAlignment = VerticalAlignment.Center,
            MinHeight = 40,
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI")
        };

        // Fokus-Effekt für TextBox
        textBox.GotFocus += (s, e) =>
        {
            textBoxBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(59, 130, 246));
            textBoxBorder.BorderThickness = new Thickness(2);
            textBoxBorder.Effect = new DropShadowEffect
            {
                Color = Color.FromRgb(59, 130, 246),
                BlurRadius = 8,
                Direction = 0,
                ShadowDepth = 0,
                Opacity = 0.3
            };
        };

        textBox.LostFocus += (s, e) =>
        {
            textBoxBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225));
            textBoxBorder.BorderThickness = new Thickness(1);
            textBoxBorder.Effect = null;
        };

        textBoxBorder.Child = textBox;

        contentPanel.Children.Add(promptText);
        contentPanel.Children.Add(textBoxBorder);
        Grid.SetRow(contentPanel, 1);

        // Button-Bereich
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(24, 0, 24, 24)
        };

        // Abbrechen-Button
        var cancelButton = new Button
        {
            Content = localizationService?.GetString("Cancel") ?? "Abbrechen",
            Width = 100,
            Height = 36,
            Margin = new Thickness(0, 0, 12, 0),
            Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
            FontWeight = FontWeights.Medium,
            FontSize = 13,
            Cursor = Cursors.Hand,
            IsCancel = true
        };

        // Style für Cancel Button
        cancelButton.Style = CreateModernButtonStyle(false);

        // OK-Button 
        var okButton = new Button
        {
            Content = localizationService?.GetString("OK") ?? "OK",
            Width = 100,
            Height = 36,
            Background = new LinearGradientBrush(
                Color.FromRgb(59, 130, 246),
                Color.FromRgb(37, 99, 235),
                90),
            BorderThickness = new Thickness(0),
            Foreground = Brushes.White,
            FontWeight = FontWeights.Medium,
            FontSize = 13,
            Cursor = Cursors.Hand,
            IsDefault = true
        };

        // Style für OK Button
        okButton.Style = CreateModernButtonStyle(true);

        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(okButton);
        Grid.SetRow(buttonPanel, 2);

        mainGrid.Children.Add(headerBorder);
        mainGrid.Children.Add(contentPanel);
        mainGrid.Children.Add(buttonPanel);
        mainBorder.Child = mainGrid;
        dialog.Content = mainBorder;

        // Event Handler
        okButton.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };
        cancelButton.Click += (s, e) => { dialog.DialogResult = false; dialog.Close(); };

        // Enter-Taste im TextBox
        textBox.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter)
            {
                dialog.DialogResult = true;
                dialog.Close();
            }
            else if (e.Key == Key.Escape)
            {
                dialog.DialogResult = false;
                dialog.Close();
            }
        };

        // Fokus auf TextBox setzen und Text auswählen
        dialog.Loaded += (s, e) => 
        { 
            textBox.Focus(); 
            textBox.SelectAll(); 
        };

        // Window kann mit Maus bewegt werden
        headerBorder.MouseLeftButtonDown += (s, e) => { dialog.DragMove(); };

        return dialog.ShowDialog() == true ? textBox.Text?.Trim() : null;
    }

    /// <summary>
    /// Erstellt einen modernen Button-Style
    /// </summary>
    private static Style CreateModernButtonStyle(bool isPrimary)
    {
        var style = new Style(typeof(Button));

        // Template für abgerundete Ecken und Hover-Effekte
        var template = new ControlTemplate(typeof(Button));
        
        var border = new FrameworkElementFactory(typeof(Border));
        border.Name = "border";
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
        border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        border.SetBinding(Border.BorderBrushProperty, new System.Windows.Data.Binding("BorderBrush") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        border.SetBinding(Border.BorderThicknessProperty, new System.Windows.Data.Binding("BorderThickness") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        
        var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        contentPresenter.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        contentPresenter.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        
        border.AppendChild(contentPresenter);
        template.VisualTree = border;

        // Hover-Trigger
        var hoverTrigger = new Trigger
        {
            Property = UIElement.IsMouseOverProperty,
            Value = true
        };

        if (isPrimary)
        {
            hoverTrigger.Setters.Add(new Setter
            {
                Property = Control.BackgroundProperty,
                Value = new LinearGradientBrush(
                    Color.FromRgb(37, 99, 235),
                    Color.FromRgb(29, 78, 216),
                    90)
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

        template.Triggers.Add(hoverTrigger);
        style.Setters.Add(new Setter(Control.TemplateProperty, template));

        return style;
    }

    /// <summary>
    /// Zeigt einen modernen Bestätigungsdialog für das Entfernen einer Gruppe an
    /// </summary>
    /// <param name="owner">Das übergeordnete Fenster</param>
    /// <param name="groupName">Name der zu entfernenden Gruppe</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>True wenn bestätigt, sonst False</returns>
    public static bool ShowRemoveGroupConfirmation(Window? owner, string groupName, LocalizationService? localizationService = null)
    {
        var title = localizationService?.GetString("RemoveGroupTitle") ?? "Gruppe entfernen";
        var message = localizationService?.GetString("RemoveGroupConfirm", groupName) ?? 
                     $"Möchten Sie die Gruppe '{groupName}' wirklich entfernen?\nAlle Spieler in dieser Gruppe werden ebenfalls entfernt.";

        return ShowModernConfirmationDialog(owner, title, message, "🗑️", true, localizationService);
    }

    /// <summary>
    /// Zeigt einen modernen Bestätigungsdialog für das Entfernen eines Spielers an
    /// </summary>
    /// <param name="owner">Das übergeordnete Fenster</param>
    /// <param name="playerName">Name des zu entfernenden Spielers</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>True wenn bestätigt, sonst False</returns>
    public static bool ShowRemovePlayerConfirmation(Window? owner, string playerName, LocalizationService? localizationService = null)
    {
        var title = localizationService?.GetString("RemovePlayerTitle") ?? "Spieler entfernen";
        var message = localizationService?.GetString("RemovePlayerConfirm", playerName) ?? 
                     $"Möchten Sie den Spieler '{playerName}' wirklich entfernen?";

        return ShowModernConfirmationDialog(owner, title, message, "👤", true, localizationService);
    }

    /// <summary>
    /// Zeigt einen modernen Bestätigungsdialog für das Zurücksetzen von Matches an
    /// </summary>
    /// <param name="owner">Das übergeordnete Fenster</param>
    /// <param name="groupName">Name der Gruppe</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>True wenn bestätigt, sonst False</returns>
    public static bool ShowResetMatchesConfirmation(Window? owner, string groupName, LocalizationService? localizationService = null)
    {
        var title = localizationService?.GetString("ResetMatchesTitle") ?? "Spiele zurücksetzen";
        var message = localizationService?.GetString("ResetMatchesConfirm", groupName) ?? 
                     $"Möchten Sie alle Spiele für Gruppe '{groupName}' wirklich zurücksetzen?\nAlle Ergebnisse gehen verloren!";

        return ShowModernConfirmationDialog(owner, title, message, "⚠️", true, localizationService);
    }

    /// <summary>
    /// Zeigt einen modernen Bestätigungsdialog für das Zurücksetzen des gesamten Turniers an
    /// </summary>
    /// <param name="owner">Das übergeordnete Fenster</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>True wenn bestätigt, sonst False</returns>
    public static bool ShowResetTournamentConfirmation(Window? owner, LocalizationService? localizationService = null)
    {
        var title = localizationService?.GetString("ResetTournament") ?? "Turnier zurücksetzen";
        var message = localizationService?.GetString("ResetTournamentConfirm") ?? 
                     "Möchten Sie wirklich das gesamte Turnier zurücksetzen?\n\n⚠ ALLE Spiele und Phasen werden gelöscht!\nNur Gruppen und Spieler bleiben erhalten.";

        return ShowModernConfirmationDialog(owner, title, message, "🔄", true, localizationService);
    }

    /// <summary>
    /// Zeigt einen modernen Bestätigungsdialog für das Zurücksetzen der KO-Phase an
    /// </summary>
    /// <param name="owner">Das übergeordnete Fenster</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>True wenn bestätigt, sonst False</returns>
    public static bool ShowResetKnockoutConfirmation(Window? owner, LocalizationService? localizationService = null)
    {
        var title = localizationService?.GetString("ResetKnockoutPhase") ?? "KO-Phase zurücksetzen";
        var message = localizationService?.GetString("ResetKnockoutConfirm") ?? 
                     "Möchten Sie wirklich die K.-o.-Phase zurücksetzen?\n\n⚠ Alle K.-o.-Spiele und der Turnierbaum werden gelöscht!\nDas Turnier wird auf die Gruppenphase zurückgesetzt.";

        return ShowModernConfirmationDialog(owner, title, message, "⚔️", true, localizationService);
    }

    /// <summary>
    /// Zeigt einen modernen Bestätigungsdialog für das Zurücksetzen der Finals-Phase an
    /// </summary>
    /// <param name="owner">Das übergeordnete Fenster</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>True wenn bestätigt, sonst False</returns>
    public static bool ShowResetFinalsConfirmation(Window? owner, LocalizationService? localizationService = null)
    {
        var title = localizationService?.GetString("ResetFinalsPhase") ?? "Finalrunde zurücksetzen";
        var message = localizationService?.GetString("ResetFinalsConfirm") ?? 
                     "Möchten Sie wirklich die Finalrunde zurücksetzen?\n\n⚠ Alle Finalspiele werden gelöscht!\nDas Turnier wird auf die Gruppenphase zurückgesetzt.";

        return ShowModernConfirmationDialog(owner, title, message, "🏆", true, localizationService);
    }

    /// <summary>
    /// Zeigt einen universellen modernen Bestätigungsdialog an
    /// </summary>
    private static bool ShowModernConfirmationDialog(Window? owner, string title, string message, string icon, bool isWarning, LocalizationService? localizationService)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 450,
            Height = 280,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = System.Windows.Media.Brushes.Transparent,
            ShowInTaskbar = false
        };

        if (owner != null)
        {
            dialog.Owner = owner;
        }

        // Hauptcontainer
        var mainBorder = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromRgb(248, 249, 250),
                Color.FromRgb(241, 243, 245),
                90),
            CornerRadius = new CornerRadius(12),
            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderThickness = new Thickness(1),
            Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 270,
                ShadowDepth = 8,
                BlurRadius = 20,
                Opacity = 0.15
            },
            Margin = new Thickness(10)
        };

        var mainGrid = new Grid();
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons

        // Header mit Farbe basierend auf Warnung
        var headerColor = isWarning 
            ? new LinearGradientBrush(Color.FromRgb(239, 68, 68), Color.FromRgb(220, 38, 38), 0)  // Rot für Warnungen
            : new LinearGradientBrush(Color.FromRgb(59, 130, 246), Color.FromRgb(37, 99, 235), 0); // Blau für Info

        var headerBorder = new Border
        {
            Background = headerColor,
            CornerRadius = new CornerRadius(12, 12, 0, 0),
            Padding = new Thickness(24, 16, 24, 16)
        };

        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        var iconText = new TextBlock
        {
            Text = icon,
            FontSize = 20,
            Margin = new Thickness(0, 0, 12, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };

        headerPanel.Children.Add(iconText);
        headerPanel.Children.Add(titleText);
        headerBorder.Child = headerPanel;
        Grid.SetRow(headerBorder, 0);

        // Content
        var contentPanel = new StackPanel
        {
            Margin = new Thickness(24, 24, 24, 20),
            VerticalAlignment = VerticalAlignment.Center
        };

        var messageText = new TextBlock
        {
            Text = message,
            FontSize = 14,
            FontWeight = FontWeights.Medium,
            Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 22,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        contentPanel.Children.Add(messageText);
        Grid.SetRow(contentPanel, 1);

        // Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(24, 0, 24, 24)
        };

        bool result = false;

        // Abbrechen Button
        var cancelButton = new Button
        {
            Content = localizationService?.GetString("Cancel") ?? "Abbrechen",
            Width = 120,
            Height = 40,
            Margin = new Thickness(0, 0, 15, 0),
            Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
            FontWeight = FontWeights.Medium,
            FontSize = 13,
            Cursor = Cursors.Hand,
            IsCancel = true
        };

        cancelButton.Style = CreateModernButtonStyle(false);
        cancelButton.Click += (s, e) => { result = false; dialog.Close(); };

        // Bestätigen Button (Farbe je nach Art)
        var confirmText = isWarning 
            ? (localizationService?.GetString("Remove") ?? "Entfernen")
            : (localizationService?.GetString("OK") ?? "OK");

        var confirmColor = isWarning
            ? new LinearGradientBrush(Color.FromRgb(239, 68, 68), Color.FromRgb(220, 38, 38), 90)  // Rot
            : new LinearGradientBrush(Color.FromRgb(59, 130, 246), Color.FromRgb(37, 99, 235), 90); // Blau

        var confirmButton = new Button
        {
            Content = confirmText,
            Width = 120,
            Height = 40,
            Margin = new Thickness(15, 0, 0, 0),
            Background = confirmColor,
            BorderThickness = new Thickness(0),
            Foreground = Brushes.White,
            FontWeight = FontWeights.Medium,
            FontSize = 13,
            Cursor = Cursors.Hand,
            IsDefault = false  // Sicherheit: Nicht default bei Lösch-Aktionen
        };

        confirmButton.Style = CreateModernButtonStyle(true);
        confirmButton.Click += (s, e) => { result = true; dialog.Close(); };

        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(confirmButton);
        Grid.SetRow(buttonPanel, 2);

        mainGrid.Children.Add(headerBorder);
        mainGrid.Children.Add(contentPanel);
        mainGrid.Children.Add(buttonPanel);
        mainBorder.Child = mainGrid;
        dialog.Content = mainBorder;

        // Window bewegbar machen
        headerBorder.MouseLeftButtonDown += (s, e) => { dialog.DragMove(); };
        
        // Escape = Cancel
        dialog.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Escape)
            {
                result = false;
                dialog.Close();
            }
        };

        dialog.ShowDialog();
        return result;
    }

    /// <summary>
    /// Erstellt einen modernen Player-Button-Style mit Hover-Effekt
    /// </summary>
    private static Style CreatePlayerButtonStyle()
    {
        var style = new Style(typeof(Button));

        var template = new ControlTemplate(typeof(Button));
        
        var border = new FrameworkElementFactory(typeof(Border));
        border.Name = "border";
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));
        border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        
        // Subtiler Schatten-Effekt
        border.SetValue(Border.EffectProperty, new DropShadowEffect
        {
            Color = Colors.Black,
            Direction = 270,
            ShadowDepth = 2,
            BlurRadius = 8,
            Opacity = 0.1
        });
        
        var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        contentPresenter.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        contentPresenter.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        
        border.AppendChild(contentPresenter);
        template.VisualTree = border;

        // Hover-Animation
        var hoverTrigger = new Trigger
        {
            Property = UIElement.IsMouseOverProperty,
            Value = true
        };

        hoverTrigger.Setters.Add(new Setter
        {
            Property = Control.EffectProperty,
            Value = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 270,
                ShadowDepth = 4,
                BlurRadius = 12,
                Opacity = 0.2
            }
        });

        template.Triggers.Add(hoverTrigger);
        style.Setters.Add(new Setter(Control.TemplateProperty, template));

        return style;
    }

    /// <summary>
    /// Zeigt eine moderne Informationsnachricht als Toast-Benachrichtigung an
    /// </summary>
    /// <param name="message">Die Nachricht</param>
    /// <param name="title">Der Titel</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <param name="owner">Das übergeordnete Fenster (optional)</param>
    public static void ShowInformation(string message, string? title = null, LocalizationService? localizationService = null, Window? owner = null)
    {
        var dialogTitle = title ?? localizationService?.GetString("Information") ?? "Information";
        
        // Versuche Toast-Benachrichtigung zu zeigen
        if (TryShowToastNotification(owner, dialogTitle, message, MessageType.Information))
        {
            return; // Toast erfolgreich angezeigt
        }
        
        // Fallback zu Modal-Dialog
        ShowModernMessageDialog(owner, dialogTitle, message, "ℹ️", MessageType.Information, localizationService);
    }

    /// <summary>
    /// Zeigt eine moderne Warnung als Toast-Benachrichtigung an
    /// </summary>
    /// <param name="message">Die Nachricht</param>
    /// <param name="title">Der Titel</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <param name="owner">Das übergeordnete Fenster (optional)</param>
    public static void ShowWarning(string message, string? title = null, LocalizationService? localizationService = null, Window? owner = null)
    {
        var dialogTitle = title ?? localizationService?.GetString("Warning") ?? "Warnung";
        
        // Versuche Toast-Benachrichtigung zu zeigen
        if (TryShowToastNotification(owner, dialogTitle, message, MessageType.Warning))
        {
            return; // Toast erfolgreich angezeigt
        }
        
        // Fallback zu Modal-Dialog
        ShowModernMessageDialog(owner, dialogTitle, message, "⚠️", MessageType.Warning, localizationService);
    }

    /// <summary>
    /// Zeigt eine moderne Fehlermeldung an - immer als Modal-Dialog da wichtiger
    /// </summary>
    /// <param name="message">Die Nachricht</param>
    /// <param name="title">Der Titel</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <param name="owner">Das übergeordnete Fenster (optional)</param>
    public static void ShowError(string message, string? title = null, LocalizationService? localizationService = null, Window? owner = null)
    {
        var dialogTitle = title ?? localizationService?.GetString("Error") ?? "Fehler";
        
        // Fehlermeldungen sind wichtig - zeige immer als Modal-Dialog
        ShowModernMessageDialog(owner, dialogTitle, message, "❌", MessageType.Error, localizationService);
    }

    /// <summary>
    /// Versucht eine Toast-Benachrichtigung anzuzeigen
    /// </summary>
    /// <param name="owner">Das übergeordnete Fenster</param>
    /// <param name="title">Titel der Nachricht</param>
    /// <param name="message">Die Nachricht</param>
    /// <param name="type">Message-Type für Farbgebung</param>
    /// <returns>True wenn Toast erfolgreich angezeigt wurde</returns>
    private static bool TryShowToastNotification(Window? owner, string title, string message, MessageType type)
    {
        try
        {
            // Suche nach dem MainGrid im aktuellen Fenster
            var parentGrid = owner?.Content as Grid;
            
            // Falls Owner nicht verfügbar, suche im Application-MainWindow
            if (parentGrid == null && Application.Current?.MainWindow != null)
            {
                parentGrid = Application.Current.MainWindow.Content as Grid;
            }
            
            if (parentGrid != null)
            {
                // Verwende farbcodierte Toast-Benachrichtigung
                ShowColoredToastNotification(parentGrid, title, message, type);
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TryShowToastNotification: ERROR: {ex.Message}");
        }
        
        return false;
    }

    /// <summary>
    /// Zeigt eine farbkodierte Toast-Benachrichtigung an
    /// </summary>
    /// <param name="parentGrid">Das übergeordnete Grid</param>
    /// <param name="title">Titel der Benachrichtigung</param>
    /// <param name="message">Nachricht der Benachrichtigung</param>
    /// <param name="type">Message-Type für Farbgebung</param>
    private static void ShowColoredToastNotification(Grid parentGrid, string title, string message, MessageType type)
    {
        try
        {
            // Farbgebung basierend auf Message-Type
            var backgroundColor = type switch
            {
                MessageType.Information => Color.FromArgb(230, 76, 175, 80),   // Grün
                MessageType.Warning => Color.FromArgb(230, 255, 193, 7),       // Orange/Gelb
                MessageType.Error => Color.FromArgb(230, 220, 53, 69),         // Rot
                _ => Color.FromArgb(230, 76, 175, 80)                          // Standard Grün
            };

            var icon = type switch
            {
                MessageType.Information => "ℹ️",
                MessageType.Warning => "⚠️",
                MessageType.Error => "❌",
                _ => "ℹ️"
            };

            // Erstelle Toast-Panel mit moderner Optik
            var toastPanel = new Border
            {
                Background = new SolidColorBrush(backgroundColor),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(16, 12, 16, 12),
                MaxWidth = 420,
                MinWidth = 300,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(20, 80, 20, 0), // Etwas weiter unten als bei UI Refresh
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 270,
                    ShadowDepth = 6,
                    BlurRadius = 12,
                    Opacity = 0.25
                }
            };

            var mainStack = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            // Icon
            var iconBlock = new TextBlock
            {
                Text = icon,
                FontSize = 16,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Top
            };

            var textStack = new StackPanel();

            // Titel
            var titleBlock = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 4)
            };

            // Nachricht
            var messageBlock = new TextBlock
            {
                Text = message,
                FontSize = 13,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18
            };

            textStack.Children.Add(titleBlock);
            textStack.Children.Add(messageBlock);

            mainStack.Children.Add(iconBlock);
            mainStack.Children.Add(textStack);
            toastPanel.Child = mainStack;

            // Füge Toast zum Grid hinzu mit hoher Z-Order
            Panel.SetZIndex(toastPanel, 9999);
            Grid.SetRowSpan(toastPanel, parentGrid.RowDefinitions.Count > 0 ? parentGrid.RowDefinitions.Count : 1);
            Grid.SetColumnSpan(toastPanel, parentGrid.ColumnDefinitions.Count > 0 ? parentGrid.ColumnDefinitions.Count : 1);
            parentGrid.Children.Add(toastPanel);

            // Fade-in Animation
            toastPanel.Opacity = 0;
            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            toastPanel.BeginAnimation(System.Windows.UIElement.OpacityProperty, fadeIn);

            // Auto-remove mit Fade-out nach angemessener Zeit
            var displayTime = type == MessageType.Error ? 7 : 5; // Fehler länger anzeigen
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(displayTime)
            };

            timer.Tick += (s, e) =>
            {
                timer.Stop();
                
                // Fade-out Animation
                var fadeOut = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(300)
                };
                
                fadeOut.Completed += (sender, args) =>
                {
                    try
                    {
                        parentGrid.Children.Remove(toastPanel);
                    }
                    catch
                    {
                        // Ignore removal errors
                    }
                };
                
                toastPanel.BeginAnimation(System.Windows.UIElement.OpacityProperty, fadeOut);
            };
            timer.Start();

            // Klick zum manuellen Schließen
            toastPanel.MouseLeftButtonUp += (s, e) =>
            {
                timer.Stop();
                try
                {
                    parentGrid.Children.Remove(toastPanel);
                }
                catch
                {
                    // Ignore removal errors
                }
            };

            // Hover-Effekt - pausiere Timer
            toastPanel.MouseEnter += (s, e) => timer.Stop();
            toastPanel.MouseLeave += (s, e) => timer.Start();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ShowColoredToastNotification: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Zeigt einen modernen Message-Dialog an
    /// </summary>
    private static void ShowModernMessageDialog(Window? owner, string title, string message, string icon, MessageType type, LocalizationService? localizationService)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 420,
            Height = 240,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = System.Windows.Media.Brushes.Transparent,
            ShowInTaskbar = false,
            Topmost = true
        };

        if (owner != null)
        {
            dialog.Owner = owner;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        // Header-Farbe je nach Message-Type
        var headerColor = type switch
        {
            MessageType.Information => new LinearGradientBrush(Color.FromRgb(34, 197, 94), Color.FromRgb(22, 163, 74), 0),   // Grün
            MessageType.Warning => new LinearGradientBrush(Color.FromRgb(245, 158, 11), Color.FromRgb(217, 119, 6), 0),     // Orange
            MessageType.Error => new LinearGradientBrush(Color.FromRgb(239, 68, 68), Color.FromRgb(220, 38, 38), 0),        // Rot
            _ => new LinearGradientBrush(Color.FromRgb(59, 130, 246), Color.FromRgb(37, 99, 235), 0)                       // Blau
        };

        // Hauptcontainer
        var mainBorder = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromRgb(248, 249, 250),
                Color.FromRgb(241, 243, 245),
                90),
            CornerRadius = new CornerRadius(12),
            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderThickness = new Thickness(1),
            Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 270,
                ShadowDepth = 12,
                BlurRadius = 25,
                Opacity = 0.2
            },
            Margin = new Thickness(10)
        };

        var mainGrid = new Grid();
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Button

        // Header
        var headerBorder = new Border
        {
            Background = headerColor,
            CornerRadius = new CornerRadius(12, 12, 0, 0),
            Padding = new Thickness(20, 14, 20, 14)
        };

        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        var iconText = new TextBlock
        {
            Text = icon,
            FontSize = 18,
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };

        headerPanel.Children.Add(iconText);
        headerPanel.Children.Add(titleText);
        headerBorder.Child = headerPanel;
        Grid.SetRow(headerBorder, 0);

        // Content
        var contentPanel = new StackPanel
        {
            Margin = new Thickness(20, 20, 20, 15),
            VerticalAlignment = VerticalAlignment.Center
        };

        var messageText = new TextBlock
        {
            Text = message,
            FontSize = 13,
            FontWeight = FontWeights.Normal,
            Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 20,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        contentPanel.Children.Add(messageText);
        Grid.SetRow(contentPanel, 1);

        // OK Button
        var buttonPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(20, 0, 20, 20)
        };

        var okButton = new Button
        {
            Content = localizationService?.GetString("OK") ?? "OK",
            Width = 100,
            Height = 36,
            Background = headerColor,
            BorderThickness = new Thickness(0),
            Foreground = Brushes.White,
            FontWeight = FontWeights.Medium,
            FontSize = 13,
            Cursor = Cursors.Hand,
            IsDefault = true
        };

        okButton.Style = CreateModernButtonStyle(true);
        okButton.Click += (s, e) => { dialog.Close(); };

        buttonPanel.Children.Add(okButton);
        Grid.SetRow(buttonPanel, 2);

        mainGrid.Children.Add(headerBorder);
        mainGrid.Children.Add(contentPanel);
        mainGrid.Children.Add(buttonPanel);
        mainBorder.Child = mainGrid;
        dialog.Content = mainBorder;

        // Window bewegbar machen
        headerBorder.MouseLeftButtonDown += (s, e) => { dialog.DragMove(); };
        
        // Enter/Escape zum Schließen
        dialog.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                dialog.Close();
            }
        };

        // Auto-Focus auf OK Button
        dialog.Loaded += (s, e) => { okButton.Focus(); };

        dialog.ShowDialog();
    }

    /// <summary>
    /// Zeigt einen Dialog zur Auswahl des Freilos-Gewinners an (Alias für Kompatibilität)
    /// </summary>
    /// <param name="owner">Das übergeordnete Fenster</param>
    /// <param name="player1Name">Name von Spieler 1</param>
    /// <param name="player2Name">Name von Spieler 2</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>1 für Spieler 1, 2 für Spieler 2, oder null wenn abgebrochen</returns>
    public static int? ShowPlayerSelectionDialog(Window? owner, string player1Name, string player2Name, LocalizationService? localizationService = null)
    {
        return ShowByeSelectionDialog(owner, player1Name, player2Name, localizationService);
    }

    /// <summary>
    /// Zeigt einen modernen Dialog zur Auswahl des Freilos-Gewinners an
    /// </summary>
    /// <param name="owner">Das übergeordnete Fenster</param>
    /// <param name="player1Name">Name von Spieler 1</param>
    /// <param name="player2Name">Name von Spieler 2</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>1 für Spieler 1, 2 für Spieler 2, oder null wenn abgebrochen</returns>
    public static int? ShowByeSelectionDialog(Window? owner, string player1Name, string player2Name, LocalizationService? localizationService = null)
    {
        var dialog = new Window
        {
            Title = localizationService?.GetString("SelectByeWinner") ?? "Freilos-Gewinner wählen",
            Width = 400,
            Height = 250,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = System.Windows.Media.Brushes.Transparent,
            ShowInTaskbar = false
        };

        if (owner != null)
        {
            dialog.Owner = owner;
        }

        // Hauptcontainer
        var mainBorder = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromRgb(248, 249, 250),
                Color.FromRgb(241, 243, 245),
                90),
            CornerRadius = new CornerRadius(12),
            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderThickness = new Thickness(1),
            Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 270,
                ShadowDepth = 8,
                BlurRadius = 20,
                Opacity = 0.15
            },
            Margin = new Thickness(10)
        };

        var mainGrid = new Grid();
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons

        // Header
        var headerBorder = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromRgb(34, 197, 94),  // Grün links
                Color.FromRgb(22, 163, 74),  // Dunkelgrün rechts
                0),
            CornerRadius = new CornerRadius(12, 12, 0, 0),
            Padding = new Thickness(24, 16, 24, 16)
        };

        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        var titleText = new TextBlock
        {
            Text = dialog.Title,
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };

        headerPanel.Children.Add(titleText);
        headerBorder.Child = headerPanel;
        Grid.SetRow(headerBorder, 0);

        // Content
        var contentPanel = new StackPanel
        {
            Margin = new Thickness(24, 24, 24, 20),
            VerticalAlignment = VerticalAlignment.Center
        };

        // Spieler 1
        var player1Panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 16)
        };

        var player1RadioButton = new RadioButton
        {
            GroupName = "ByePlayer",
            Margin = new Thickness(0, 0, 12, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Style = (Style)Application.Current.Resources["ModernRadioButton"]
        };

        var player1Text = new TextBlock
        {
            Text = player1Name,
            FontSize = 14,
            FontWeight = FontWeights.Medium,
            Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
            VerticalAlignment = VerticalAlignment.Center
        };

        player1Panel.Children.Add(player1RadioButton);
        player1Panel.Children.Add(player1Text);

        contentPanel.Children.Add(player1Panel);

        // Spieler 2
        var player2Panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 24)
        };

        var player2RadioButton = new RadioButton
        {
            GroupName = "ByePlayer",
            Margin = new Thickness(0, 0, 12, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Style = (Style)Application.Current.Resources["ModernRadioButton"]
        };

        var player2Text = new TextBlock
        {
            Text = player2Name,
            FontSize = 14,
            FontWeight = FontWeights.Medium,
            Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
            VerticalAlignment = VerticalAlignment.Center
        };

        player2Panel.Children.Add(player2RadioButton);
        player2Panel.Children.Add(player2Text);

        contentPanel.Children.Add(player2Panel);
        Grid.SetRow(contentPanel, 1);

        // Button-Bereich
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(24, 0, 24, 24)
        };

        // Abbrechen-Button
        var cancelButton = new Button
        {
            Content = localizationService?.GetString("Cancel") ?? "Abbrechen",
            Width = 100,
            Height = 36,
            Margin = new Thickness(0, 0, 12, 0),
            Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
            FontWeight = FontWeights.Medium,
            FontSize = 13,
            Cursor = Cursors.Hand,
            IsCancel = true
        };

        // Style für Cancel Button
        cancelButton.Style = CreateModernButtonStyle(false);

        // OK-Button 
        var okButton = new Button
        {
            Content = localizationService?.GetString("OK") ?? "OK",
            Width = 100,
            Height = 36,
            Background = new LinearGradientBrush(
                Color.FromRgb(59, 130, 246),
                Color.FromRgb(37, 99, 235),
                90),
            BorderThickness = new Thickness(0),
            Foreground = Brushes.White,
            FontWeight = FontWeights.Medium,
            FontSize = 13,
            Cursor = Cursors.Hand,
            IsDefault = true
        };

        // Style für OK Button
        okButton.Style = CreateModernButtonStyle(true);

        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(okButton);
        Grid.SetRow(buttonPanel, 2);

        mainGrid.Children.Add(headerBorder);
        mainGrid.Children.Add(contentPanel);
        mainGrid.Children.Add(buttonPanel);
        mainBorder.Child = mainGrid;
        dialog.Content = mainBorder;

        // Event Handler
        okButton.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };
        cancelButton.Click += (s, e) => { dialog.DialogResult = false; dialog.Close(); };

        // Enter-Taste im TextBox
        player1RadioButton.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter)
            {
                dialog.DialogResult = true;
                dialog.Close();
            }
            else if (e.Key == Key.Escape)
            {
                dialog.DialogResult = false;
                dialog.Close();
            }
        };

        player2RadioButton.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter)
            {
                dialog.DialogResult = true;
                dialog.Close();
            }
            else if (e.Key == Key.Escape)
            {
                dialog.DialogResult = false;
                dialog.Close();
            }
        };

        // Window kann mit Maus bewegt werden
        headerBorder.MouseLeftButtonDown += (s, e) => { dialog.DragMove(); };

        return dialog.ShowDialog() == true 
            ? (player1RadioButton.IsChecked == true ? 1 : 2) 
            : (int?)null;
    }

    /// <summary>
    /// Message-Type für verschiedene Dialog-Arten
    /// </summary>
    public enum MessageType
    {
        Information,
        Warning,
        Error
    }
}