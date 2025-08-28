using System.Windows.Controls;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Translation Manager für TournamentTab - verwaltet alle Übersetzungen
/// </summary>
public class TournamentTabTranslationManager
{
    private readonly LocalizationService _localizationService;

    // UI Elements für Übersetzungen - werden vom TournamentTab gesetzt
    public TabItem? SetupTabItem { get; set; }
    public TabItem? GroupPhaseTabItem { get; set; }
    public TabItem? FinalsTabItem { get; set; }
    public TabItem? KnockoutTabItem { get; set; }
    public Button? ConfigureRulesButton { get; set; }
    public Button? AddGroupButton { get; set; }
    public Button? RemoveGroupButton { get; set; }
    public Button? AddPlayerButton { get; set; }
    public Button? RemovePlayerButton { get; set; }
    public Button? GenerateMatchesButton { get; set; }
    public Button? ResetMatchesButton { get; set; }
    public Button? AdvanceToNextPhaseButton { get; set; }
    public Button? ResetTournamentButton { get; set; }
    public Button? ResetKnockoutButton { get; set; }
    public Button? ResetFinalsButton { get; set; }
    public Button? RefreshUIButton { get; set; }
    public TextBlock? GroupsHeaderText { get; set; }
    public TextBlock? MatchesHeaderText { get; set; }
    public TextBlock? StandingsHeaderText { get; set; }
    public TextBlock? SelectGroupText { get; set; }
    public TextBlock? TournamentOverviewHeader { get; set; }
    public TabItem? GamesTabItem { get; set; }
    public TabItem? TableTabItem { get; set; }
    public DataGrid? MatchesDataGrid { get; set; }
    public DataGrid? FinalsMatchesDataGrid { get; set; }
    public DataGrid? KnockoutMatchesDataGrid { get; set; }
    public DataGrid? StandingsDataGrid { get; set; }

    public TournamentTabTranslationManager(LocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public void UpdateTranslations()
    {
        UpdateTabHeaders();
        UpdateButtonTexts();
        UpdateHeaderTexts();
        UpdateDataGridHeaders();
    }

    private void UpdateTabHeaders()
    {
        if (SetupTabItem != null)
            SetupTabItem.Header = _localizationService.GetString("SetupTab");
        if (GroupPhaseTabItem != null)
            GroupPhaseTabItem.Header = _localizationService.GetString("GroupPhaseTab");
        if (FinalsTabItem != null)
            FinalsTabItem.Header = _localizationService.GetString("FinalsTab");
        if (KnockoutTabItem != null)
            KnockoutTabItem.Header = _localizationService.GetString("KnockoutTab");
        if (GamesTabItem != null)
            GamesTabItem.Header = _localizationService.GetString("Matches");
        if (TableTabItem != null)
            TableTabItem.Header = _localizationService.GetString("Standings");
    }

    private void UpdateButtonTexts()
    {
        if (ConfigureRulesButton != null)
            ConfigureRulesButton.Content = _localizationService.GetString("ConfigureRules");
        if (AddGroupButton != null)
            AddGroupButton.Content = _localizationService.GetString("AddGroup");
        if (RemoveGroupButton != null)
            RemoveGroupButton.Content = _localizationService.GetString("RemoveGroup");
        if (AddPlayerButton != null)
            AddPlayerButton.Content = _localizationService.GetString("AddPlayer");
        if (RemovePlayerButton != null)
            RemovePlayerButton.Content = _localizationService.GetString("RemovePlayer");
        if (GenerateMatchesButton != null)
            GenerateMatchesButton.Content = _localizationService.GetString("GenerateMatches");
        if (ResetMatchesButton != null)
            ResetMatchesButton.Content = "⚠ " + _localizationService.GetString("ResetMatches");
        if (AdvanceToNextPhaseButton != null)
            AdvanceToNextPhaseButton.Content = "🏆 " + _localizationService.GetString("AdvanceToNextPhase");
        if (ResetTournamentButton != null)
            ResetTournamentButton.Content = "🔄 " + _localizationService.GetString("ResetTournament");
        if (ResetKnockoutButton != null)
            ResetKnockoutButton.Content = "⚠ " + _localizationService.GetString("ResetKnockoutPhase");
        if (ResetFinalsButton != null)
            ResetFinalsButton.Content = "⚠ " + _localizationService.GetString("ResetFinalsPhase");
        
        if (RefreshUIButton != null)
        {
            RefreshUIButton.Content = "🔄 " + _localizationService.GetString("RefreshUI");
            RefreshUIButton.ToolTip = _localizationService.GetString("RefreshUITooltip");
        }
    }

    private void UpdateHeaderTexts()
    {
        if (GroupsHeaderText != null)
            GroupsHeaderText.Text = _localizationService.GetString("Groups");
        if (MatchesHeaderText != null)
            MatchesHeaderText.Text = _localizationService.GetString("Matches");
        if (StandingsHeaderText != null)
            StandingsHeaderText.Text = _localizationService.GetString("Standings");
        if (SelectGroupText != null)
            SelectGroupText.Text = _localizationService.GetString("SelectGroup");
        if (TournamentOverviewHeader != null)
            TournamentOverviewHeader.Text = _localizationService.GetString("TournamentOverview");
    }

    private void UpdateDataGridHeaders()
    {
        if (MatchesDataGrid?.Columns.Count >= 3)
        {
            MatchesDataGrid.Columns[0].Header = _localizationService.GetString("Match") ?? "Match";
            MatchesDataGrid.Columns[1].Header = _localizationService.GetString("Result");
            MatchesDataGrid.Columns[2].Header = _localizationService.GetString("Status") ?? "Status";
        }

        if (FinalsMatchesDataGrid?.Columns.Count >= 3)
        {
            FinalsMatchesDataGrid.Columns[0].Header = _localizationService.GetString("Match") ?? "Match";
            FinalsMatchesDataGrid.Columns[1].Header = _localizationService.GetString("Result");
            FinalsMatchesDataGrid.Columns[2].Header = _localizationService.GetString("Status") ?? "Status";
        }

        if (KnockoutMatchesDataGrid?.Columns.Count >= 4)
        {
            KnockoutMatchesDataGrid.Columns[0].Header = _localizationService.GetString("Round") ?? "Runde";
            KnockoutMatchesDataGrid.Columns[1].Header = _localizationService.GetString("Match") ?? "Match";
            KnockoutMatchesDataGrid.Columns[2].Header = _localizationService.GetString("Result");
            KnockoutMatchesDataGrid.Columns[3].Header = _localizationService.GetString("Status") ?? "Status";
        }

        if (StandingsDataGrid?.Columns.Count >= 6)
        {
            StandingsDataGrid.Columns[0].Header = _localizationService.GetString("Position") ?? "Pos";
            StandingsDataGrid.Columns[1].Header = _localizationService.GetString("Player");
            StandingsDataGrid.Columns[2].Header = _localizationService.GetString("Score");
            StandingsDataGrid.Columns[3].Header = "W-D-L";
            StandingsDataGrid.Columns[4].Header = _localizationService.GetString("Sets");
            StandingsDataGrid.Columns[5].Header = _localizationService.GetString("Legs");
        }
    }
}