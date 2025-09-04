using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views;

public partial class HelpWindow : Window
{
    private readonly LocalizationService _localizationService;
    private Dictionary<TreeViewItem, string> _contentMapping = null!;

    public HelpWindow(LocalizationService localizationService)
    {
        InitializeComponent();
        _localizationService = localizationService;
        
        // Initialize the content mapping AFTER InitializeComponent() is called
        InitializeContentMapping();
        InitializeTranslations();
        ShowInitialContent();

        // Subscribe to language changes
        _localizationService.PropertyChanged += (s, e) => InitializeTranslations();
    }

    private void InitializeContentMapping()
    {
        // Map TreeView items to their content keys
        _contentMapping = new Dictionary<TreeViewItem, string>
        {
            { GeneralItem, "HelpGeneralContent" },
            { TournamentSetupItem, "HelpTournamentSetupContent" },
            { GroupManagementItem, "HelpGroupManagementContent" },
            { GameRulesItem, "HelpGameRulesContent" },
            { MatchesItem, "HelpMatchesContent" },
            { TournamentPhasesItem, "HelpTournamentPhasesContent" },
            { MenusItem, "HelpMenusContent" },
            { LicenseSystemItem, "HelpLicenseSystemContent" },
            { ApiIntegrationItem, "HelpApiIntegrationContent" },
            { TournamentHubItem, "HelpTournamentHubContent" },
            { StatisticsItem, "HelpStatisticsContent" },
            { PrintingItem, "HelpPrintingContent" },
            { TipsItem, "HelpTipsContent" }
        };
    }

    private void InitializeTranslations()
    {
        // Update window title
        Title = _localizationService.GetString("HelpTitle");
        TitleTextBlock.Text = _localizationService.GetString("HelpTitle");

        // Update navigation items
        GeneralItem.Header = _localizationService.GetString("HelpGeneral");
        TournamentSetupItem.Header = _localizationService.GetString("HelpTournamentSetup");
        GroupManagementItem.Header = _localizationService.GetString("HelpGroupManagement");
        GameRulesItem.Header = _localizationService.GetString("HelpGameRules");
        MatchesItem.Header = _localizationService.GetString("HelpMatches");
        TournamentPhasesItem.Header = _localizationService.GetString("HelpTournamentPhases");
        MenusItem.Header = _localizationService.GetString("HelpMenus");
        LicenseSystemItem.Header = _localizationService.GetString("HelpLicenseSystem");
        ApiIntegrationItem.Header = _localizationService.GetString("HelpApiIntegration");
        TournamentHubItem.Header = _localizationService.GetString("HelpTournamentHub");
        StatisticsItem.Header = _localizationService.GetString("HelpStatistics");
        PrintingItem.Header = _localizationService.GetString("HelpPrinting");
        TipsItem.Header = _localizationService.GetString("HelpTips");

        // Update button
        CloseButton.Content = _localizationService.GetString("Close");

        // Update content if an item is selected
        var selectedItem = HelpNavigationTree.SelectedItem as TreeViewItem;
        if (selectedItem != null && _contentMapping != null && _contentMapping.ContainsKey(selectedItem))
        {
            ContentTextBlock.Text = _localizationService.GetString(_contentMapping[selectedItem]);
        }
    }

    private void ShowInitialContent()
    {
        // Show the general help content initially
        ContentTextBlock.Text = _localizationService.GetString("HelpGeneralContent");
    }

    private void HelpNavigationTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        var selectedItem = e.NewValue as TreeViewItem;
        if (selectedItem != null && _contentMapping != null && _contentMapping.ContainsKey(selectedItem))
        {
            var contentKey = _contentMapping[selectedItem];
            ContentTextBlock.Text = _localizationService.GetString(contentKey);
            
            // Scroll to top
            ContentScrollViewer.ScrollToTop();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}