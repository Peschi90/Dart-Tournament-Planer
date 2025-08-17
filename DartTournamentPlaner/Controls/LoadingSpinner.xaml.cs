using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Controls;

/// <summary>
/// Wiederverwendbarer Loading Spinner mit Overlay für lange dauernde Operationen
/// </summary>
public partial class LoadingSpinner : UserControl, INotifyPropertyChanged
{
    private Storyboard? _spinnerAnimation;
    private string _loadingText = "Wird geladen...";
    private string _progressText = "";
    private bool _isVisible = false;
    private LocalizationService? _localizationService;

    public static readonly DependencyProperty LoadingTextProperty = 
        DependencyProperty.Register(nameof(LoadingText), typeof(string), typeof(LoadingSpinner), 
            new PropertyMetadata("Wird geladen...", OnLoadingTextChanged));

    public static readonly DependencyProperty ProgressTextProperty = 
        DependencyProperty.Register(nameof(ProgressText), typeof(string), typeof(LoadingSpinner), 
            new PropertyMetadata("", OnProgressTextChanged));

    public static readonly DependencyProperty IsSpinnerVisibleProperty = 
        DependencyProperty.Register(nameof(IsSpinnerVisible), typeof(bool), typeof(LoadingSpinner), 
            new PropertyMetadata(false, OnVisibilityChanged));

    public string LoadingText
    {
        get => (string)GetValue(LoadingTextProperty);
        set => SetValue(LoadingTextProperty, value);
    }

    public string ProgressText
    {
        get => (string)GetValue(ProgressTextProperty);
        set => SetValue(ProgressTextProperty, value);
    }

    public bool IsSpinnerVisible
    {
        get => (bool)GetValue(IsSpinnerVisibleProperty);
        set => SetValue(IsSpinnerVisibleProperty, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public LoadingSpinner()
    {
        InitializeComponent();
        DataContext = this;
        
        Loaded += LoadingSpinner_Loaded;
        UpdateVisibility();
    }

    private void LoadingSpinner_Loaded(object sender, RoutedEventArgs e)
    {
        // Animation Resource laden
        if (Resources["SpinnerAnimation"] is Storyboard storyboard)
        {
            _spinnerAnimation = storyboard;
        }
    }

    private static void OnLoadingTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LoadingSpinner spinner)
        {
            spinner._loadingText = (string)e.NewValue;
            spinner.LoadingTextBlock.Text = spinner._loadingText;
        }
    }

    private static void OnProgressTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LoadingSpinner spinner)
        {
            spinner._progressText = (string)e.NewValue;
            spinner.ProgressTextBlock.Text = spinner._progressText;
            spinner.ProgressTextBlock.Visibility = string.IsNullOrEmpty(spinner._progressText) 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }
    }

    private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LoadingSpinner spinner)
        {
            spinner._isVisible = (bool)e.NewValue;
            spinner.UpdateVisibility();
        }
    }

    private void UpdateVisibility()
    {
        if (_isVisible)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void UpdateTranslations()
    {
        // Standard Loading Text falls nicht spezifisch gesetzt
        if (_loadingText == "Wird geladen..." || _loadingText == "Loading...")
        {
            var newText = "Überprüfe Gruppenstatus..."; // Hardcoded fallback
            LoadingText = newText;
        }
    }

    /// <summary>
    /// Zeigt den Loading Spinner an
    /// </summary>
    public void Show()
    {
        try
        {
            Visibility = Visibility.Visible;
            _isVisible = true;

            // Starte Animation
            if (_spinnerAnimation != null && SpinnerEllipse != null)
            {
                _spinnerAnimation.Begin(SpinnerEllipse);
            }

            System.Diagnostics.Debug.WriteLine("LoadingSpinner: Shown");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadingSpinner.Show: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Versteckt den Loading Spinner
    /// </summary>
    public void Hide()
    {
        try
        {
            Visibility = Visibility.Collapsed;
            _isVisible = false;

            // Stoppe Animation
            if (_spinnerAnimation != null && SpinnerEllipse != null)
            {
                _spinnerAnimation.Stop(SpinnerEllipse);
            }

            System.Diagnostics.Debug.WriteLine("LoadingSpinner: Hidden");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadingSpinner.Hide: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Zeigt den Spinner für eine bestimmte Zeit an
    /// </summary>
    /// <param name="duration">Anzeigedauer in Millisekunden</param>
    public async Task ShowForDuration(int duration = 1000)
    {
        Show();
        await Task.Delay(duration);
        Hide();
    }

    /// <summary>
    /// Führt eine Operation aus während der Spinner angezeigt wird
    /// </summary>
    /// <param name="operation">Die auszuführende Operation</param>
    /// <param name="loadingText">Optionaler Loading Text</param>
    /// <param name="progressCallback">Optional Progress Callback</param>
    public async Task ExecuteWithSpinner(Func<IProgress<string>, Task> operation, 
                                        string? loadingText = null, 
                                        Action<string>? progressCallback = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(loadingText))
            {
                LoadingText = loadingText;
            }

            Show();

            var progress = new Progress<string>(message =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ProgressText = message;
                    progressCallback?.Invoke(message);
                }, DispatcherPriority.DataBind);
            });

            await operation(progress);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadingSpinner.ExecuteWithSpinner: ERROR: {ex.Message}");
            throw;
        }
        finally
        {
            Hide();
        }
    }

    /// <summary>
    /// Führt eine synchrone Operation aus während der Spinner angezeigt wird
    /// </summary>
    /// <param name="operation">Die auszuführende Operation</param>
    /// <param name="loadingText">Optionaler Loading Text</param>
    public async Task ExecuteWithSpinner(Action operation, string? loadingText = null)
    {
        await ExecuteWithSpinner(async (progress) =>
        {
            await Task.Run(() => operation());
        }, loadingText);
    }
}

/// <summary>
/// Extension Methods für einfache Verwendung des LoadingSpinners
/// </summary>
public static class LoadingSpinnerExtensions
{
    /// <summary>
    /// Erstellt und zeigt einen LoadingSpinner auf dem angegebenen Container
    /// </summary>
    /// <param name="container">Container (Grid, Canvas, etc.) für den Overlay</param>
    /// <param name="loadingText">Loading Text</param>
    /// <returns>LoadingSpinner Instanz</returns>
    public static LoadingSpinner ShowLoadingSpinner(this Panel container, string? loadingText = null)
    {
        var spinner = new LoadingSpinner();
        
        if (!string.IsNullOrEmpty(loadingText))
        {
            spinner.LoadingText = loadingText;
        }

        // Füge Spinner zum Container hinzu
        if (container is Grid grid)
        {
            Grid.SetRowSpan(spinner, grid.RowDefinitions.Count > 0 ? grid.RowDefinitions.Count : 1);
            Grid.SetColumnSpan(spinner, grid.ColumnDefinitions.Count > 0 ? grid.ColumnDefinitions.Count : 1);
        }
        
        container.Children.Add(spinner);
        spinner.Show();
        
        return spinner;
    }

    /// <summary>
    /// Entfernt einen LoadingSpinner vom Container
    /// </summary>
    /// <param name="container">Container</param>
    /// <param name="spinner">LoadingSpinner Instanz</param>
    public static void HideLoadingSpinner(this Panel container, LoadingSpinner spinner)
    {
        spinner.Hide();
        container.Children.Remove(spinner);
    }
}