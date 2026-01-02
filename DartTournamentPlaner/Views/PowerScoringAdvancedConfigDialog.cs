using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DartTournamentPlaner.Models.PowerScore;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views;

/// <summary>
/// Dialog für erweiterte Gruppenverteilungs-Einstellungen
/// </summary>
public partial class PowerScoringAdvancedConfigDialog : Window
{
    public GroupDistributionConfig Config { get; private set; }
    public bool DialogResult { get; private set; }
    
    private readonly LocalizationService _localizationService;
    private readonly ObservableCollection<ClassRuleViewModel> _classRules = new();

    public PowerScoringAdvancedConfigDialog(GroupDistributionConfig currentConfig, LocalizationService localizationService)
    {
        InitializeComponent();
        
        Config = currentConfig;
        _localizationService = localizationService;
        
        LoadConfiguration();
        UpdateTranslations();
    }
    
    private void UpdateTranslations()
    {
        try
        {
            // Window Title
            Title = _localizationService.GetString("PowerScoring_AdvancedSettings_Title");
            
            // ✅ NEU: Header Title (im XAML mit x:Name)
            if (FindName("HeaderTitleText") is TextBlock headerTitle)
            {
                headerTitle.Text = _localizationService.GetString("PowerScoring_AdvancedSettings_Title");
            }
            
            // ✅ NEU: Section Titles
            if (FindName("DistributionModeTitleText") is TextBlock distModeTitle)
            {
                distModeTitle.Text = _localizationService.GetString("PowerScoring_AdvancedSettings_DistributionMode");
            }
            
            if (FindName("ClassRulesTitleText") is TextBlock classRulesTitle)
            {
                classRulesTitle.Text = _localizationService.GetString("PowerScoring_AdvancedSettings_ClassRules");
            }
            
            // Buttons
            ApplyButton.Content = _localizationService.GetString("PowerScoring_AdvancedSettings_Apply");
            CancelButton.Content = _localizationService.GetString("PowerScoring_AdvancedSettings_Cancel");
            
            // ComboBox Items
            var comboItems = DistributionModeCombo.Items.OfType<ComboBoxItem>().ToList();
            if (comboItems.Count >= 4)
            {
                comboItems[0].Content = _localizationService.GetString("PowerScoring_AdvancedSettings_Mode_Balanced");
                comboItems[1].Content = _localizationService.GetString("PowerScoring_AdvancedSettings_Mode_SnakeDraft");
                comboItems[2].Content = _localizationService.GetString("PowerScoring_AdvancedSettings_Mode_TopHeavy");
                comboItems[3].Content = _localizationService.GetString("PowerScoring_AdvancedSettings_Mode_Random");
            }
            
            // Mode Description
            UpdateModeDescriptionText();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"?? Error updating translations: {ex.Message}");
        }
    }
    
    private void UpdateModeDescriptionText()
    {
        var selectedItem = DistributionModeCombo.SelectedItem as ComboBoxItem;
        var mode = selectedItem?.Tag?.ToString() ?? "Balanced";
        
        ModeDescriptionText.Text = mode switch
        {
            "Balanced" => _localizationService.GetString("PowerScoring_AdvancedSettings_Mode_Balanced_Desc"),
            "SnakeDraft" => _localizationService.GetString("PowerScoring_AdvancedSettings_Mode_SnakeDraft_Desc"),
            "TopHeavy" => _localizationService.GetString("PowerScoring_AdvancedSettings_Mode_TopHeavy_Desc"),
            "Random" => _localizationService.GetString("PowerScoring_AdvancedSettings_Mode_Random_Desc"),
            _ => ""
        };
    }
    
    private void LoadConfiguration()
    {
        // Distribution Mode
        var modeIndex = Config.Mode switch
        {
            DistributionMode.Balanced => 0,
            DistributionMode.SnakeDraft => 1,
            DistributionMode.TopHeavy => 2,
            DistributionMode.Random => 3,
            _ => 0
        };
        DistributionModeCombo.SelectedIndex = modeIndex;
        
        // Player Limits (versteckt aber noch vorhanden)
        MinPlayersTextBox.Text = Config.MinPlayersPerGroup.ToString();
        MaxPlayersTextBox.Text = Config.MaxPlayersPerGroup.ToString();
        
        // Class Rules
        foreach (var className in Config.SelectedClasses)
        {
            var hasRule = Config.ClassRules.TryGetValue(className, out var existingRule);
            
            _classRules.Add(new ClassRuleViewModel
            {
                ClassName = GetClassDisplayName(className),
                ClassKey = className,
                GroupCount = existingRule?.CustomGroupCount ?? Config.GroupsPerClass,
                PlayersPerGroup = existingRule?.PlayersPerGroup ?? Config.PlayersPerGroup,
                Skip = existingRule?.Skip ?? false
            });
        }
        
        ClassRulesControl.ItemsSource = _classRules;
    }
    
    private string GetClassDisplayName(string className)
    {
        return className switch
        {
            "Platin" => "?? Platin",
            "Gold" => "?? Gold",
            "Silber" => "?? Silber",
            "Bronze" => "?? Bronze",
            "Eisen" => "?? Eisen",
            _ => className
        };
    }
    
    private void DistributionModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        UpdateModeDescriptionText();
    }
    
    private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !int.TryParse(e.Text, out _);
    }
    
    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Distribution Mode
            var selectedMode = ((ComboBoxItem)DistributionModeCombo.SelectedItem)?.Tag?.ToString();
            Config.Mode = selectedMode switch
            {
                "SnakeDraft" => DistributionMode.SnakeDraft,
                "TopHeavy" => DistributionMode.TopHeavy,
                "Random" => DistributionMode.Random,
                _ => DistributionMode.Balanced
            };
            
            // Class Rules
            Config.ClassRules.Clear();
            foreach (var rule in _classRules)
            {
                if (rule.GroupCount != Config.GroupsPerClass || 
                    rule.PlayersPerGroup != Config.PlayersPerGroup || 
                    rule.Skip)
                {
                    Config.ClassRules[rule.ClassKey] = new ClassSpecificRules
                    {
                        CustomGroupCount = rule.GroupCount,
                        PlayersPerGroup = rule.PlayersPerGroup,
                        Skip = rule.Skip
                    };
                }
            }
            
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
    
    public static bool ShowDialog(GroupDistributionConfig config, LocalizationService localizationService, Window? owner = null)
    {
        var dialog = new PowerScoringAdvancedConfigDialog(config, localizationService);
        if (owner != null && owner.IsLoaded)
        {
            dialog.Owner = owner;
        }
        dialog.ShowDialog();
        return dialog.DialogResult;
    }
}

/// <summary>
/// ViewModel für Klassen-Regeln in der UI
/// </summary>
public class ClassRuleViewModel : INotifyPropertyChanged
{
    private int _groupCount;
    private int _playersPerGroup;
    private bool _skip;
    
    public string ClassName { get; set; } = "";
    public string ClassKey { get; set; } = "";
    
    public int GroupCount
    {
        get => _groupCount;
        set
        {
            if (_groupCount != value)
            {
                _groupCount = value;
                OnPropertyChanged(nameof(GroupCount));
            }
        }
    }
    
    public int PlayersPerGroup
    {
        get => _playersPerGroup;
        set
        {
            if (_playersPerGroup != value)
            {
                _playersPerGroup = value;
                OnPropertyChanged(nameof(PlayersPerGroup));
            }
        }
    }
    
    public bool Skip
    {
        get => _skip;
        set
        {
            if (_skip != value)
            {
                _skip = value;
                OnPropertyChanged(nameof(Skip));
            }
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
