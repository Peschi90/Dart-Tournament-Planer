using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.PlannerImport;

namespace DartTournamentPlaner.Views
{
    public partial class PlannerImportReviewDialog : Window
    {
        public PlannerTournamentSummary Summary { get; }
        public ObservableCollection<PlannerParticipant> Participants { get; }
        public ObservableCollection<PlannerClassInfo> Classes { get; }
        public string GameRulesText { get; private set; } = string.Empty;

        private readonly PlannerImportService _importService;
        private readonly LocalizationService? _localizationService;

        public PlannerImportReviewDialog(PlannerTournamentSummary summary, IEnumerable<PlannerParticipant> participants, TournamentManagementService tournamentService, LocalizationService? localizationService)
        {
            Summary = summary ?? throw new ArgumentNullException(nameof(summary));
            Participants = new ObservableCollection<PlannerParticipant>(participants ?? Enumerable.Empty<PlannerParticipant>());
            Classes = new ObservableCollection<PlannerClassInfo>(summary.Classes ?? new());
            _localizationService = localizationService;
            _importService = new PlannerImportService(tournamentService, App.DataService!, localizationService);

            GameRulesText = FormatGameRules(summary.GameRules);

            InitializeComponent();
            DataContext = this;

            ApplyLocalization();
            GameRulesTextBlock.Text = GameRulesText;
        }

        private void ApplyLocalization()
        {
            try
            {
                HeaderTitle.Text = _localizationService?.GetString("PlannerImportReviewTitle") ?? HeaderTitle.Text;
                HeaderSubtitle.Text = _localizationService?.GetString("PlannerImportReviewSubtitle") ?? HeaderSubtitle.Text;
                MetadataLabel.Text = _localizationService?.GetString("PlannerImportReviewMetadata") ?? MetadataLabel.Text;
                ClassesLabel.Text = _localizationService?.GetString("PlannerImportReviewClasses") ?? ClassesLabel.Text;
                GameRulesLabel.Text = _localizationService?.GetString("PlannerImportReviewGameRules") ?? GameRulesLabel.Text;
                PlayersLabel.Text = _localizationService?.GetString("PlannerImportReviewPlayers") ?? PlayersLabel.Text;
                BackButton.Content = _localizationService?.GetString("PlannerImportReviewBack") ?? BackButton.Content;
                ImportButton.Content = _localizationService?.GetString("PlannerImportReviewImport") ?? ImportButton.Content;
                Title = HeaderTitle.Text;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"?? Error applying localization in review dialog: {ex.Message}");
            }
        }

        private string FormatGameRules(object? gameRules)
        {
            if (gameRules == null) return _localizationService?.GetString("PlannerFetchStatusEmpty") ?? "Keine Spielregeln";

            try
            {
                var normalized = JsonSerializer.Serialize(gameRules);
                var element = JsonSerializer.Deserialize<JsonElement>(normalized);
                var sb = new StringBuilder();
                RenderJson(element, sb, 0);
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"?? Error formatting game rules: {ex.Message}");
                return gameRules.ToString() ?? string.Empty;
            }
        }

        private void RenderJson(JsonElement element, StringBuilder sb, int indent)
        {
            var pad = new string(' ', indent);
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in element.EnumerateObject())
                    {
                        if (prop.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                        {
                            sb.AppendLine($"{pad}{prop.Name}:");
                            RenderJson(prop.Value, sb, indent + 2);
                        }
                        else
                        {
                            sb.AppendLine($"{pad}{prop.Name}: {prop.Value}");
                        }
                    }
                    break;
                case JsonValueKind.Array:
                    int idx = 1;
                    foreach (var item in element.EnumerateArray())
                    {
                        sb.AppendLine($"{pad}- Item {idx}:");
                        RenderJson(item, sb, indent + 2);
                        idx++;
                    }
                    break;
                default:
                    sb.AppendLine($"{pad}{element}");
                    break;
            }
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ImportButton.IsEnabled = false;
                var success = await _importService.ImportAsync(Summary, Participants, Owner ?? this);
                if (success)
                {
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error importing tournament: {ex.Message}");
                MessageBox.Show(ex.Message, _localizationService?.GetString("Error") ?? "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ImportButton.IsEnabled = true;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
