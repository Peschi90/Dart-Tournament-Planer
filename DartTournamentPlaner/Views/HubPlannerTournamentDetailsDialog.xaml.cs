using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views
{
    public partial class HubPlannerTournamentDetailsDialog : Window
    {
        private PlannerTournamentSummary _summary;
        private readonly LocalizationService? _localizationService;

        public HubPlannerTournamentDetailsDialog()
        {
            InitializeComponent();
        }

        private HubPlannerTournamentDetailsDialog(PlannerTournamentSummary summary, LocalizationService? localizationService) : this()
        {
            _summary = summary ?? throw new ArgumentNullException(nameof(summary));
            _localizationService = localizationService;
            PopulateFields();
        }

        public static void ShowDialog(Window owner, PlannerTournamentSummary summary, LocalizationService? localizationService)
        {
            ArgumentNullException.ThrowIfNull(summary);

            var dialog = new HubPlannerTournamentDetailsDialog(summary, localizationService)
            {
                Owner = owner
            };

            dialog.ShowDialog();
        }

        private string L(string key, string fallback) => _localizationService?.GetString(key) ?? fallback;

        private void PopulateFields()
        {
            try
            {
                TitleText.Text = _summary.Name ?? string.Empty;
                SubtitleText.Text = _summary.Description ?? string.Empty;

                NameText.Text = _summary.Name ?? "";
                IdText.Text = _summary.TournamentId ?? "";
                LicenseText.Text = _summary.LicenseKey ?? "";
                StatusText.Text = _summary.Status ?? "";
                ModeText.Text = _summary.GameMode ?? "";
                LocationText.Text = _summary.Location ?? "";
                StartText.Text = _summary.StartDate ?? "";
                UpdatedText.Text = _summary.UpdatedAt ?? "";
                CreatedText.Text = _summary.CreatedAt ?? "";

                DescriptionTextBlock.Text = _summary.Description ?? string.Empty;

                GameRulesTextBlock.Text = FormatGameRules(_summary.GameRules);

                CopyIdButton.Content = L("PlannerFetchCopyId", "Copy ID");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Error populating details dialog: {ex.Message}");
            }
        }

        private string FormatGameRules(object? gameRules)
        {
            if (gameRules == null) return L("PlannerFetchStatusEmpty", "No game rules available.");

            try
            {
                // Normalize to JsonElement for traversal
                var normalized = JsonSerializer.Serialize(gameRules);
                var element = JsonSerializer.Deserialize<JsonElement>(normalized);

                if (element.ValueKind == JsonValueKind.Array)
                {
                    var sb = new StringBuilder();
                    int idx = 1;
                    foreach (var item in element.EnumerateArray())
                    {
                        sb.AppendLine($"Rule Set #{idx}");
                        sb.AppendLine(FormatJsonElement(item, 2));
                        sb.AppendLine();
                        idx++;
                    }
                    return sb.ToString().TrimEnd();
                }

                if (element.ValueKind == JsonValueKind.Object)
                {
                    return FormatJsonElement(element, 0);
                }

                return element.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Error formatting game rules: {ex.Message}");
                return gameRules.ToString() ?? string.Empty;
            }
        }

        private string FormatJsonElement(JsonElement element, int indent)
        {
            var sb = new StringBuilder();
            var pad = new string(' ', indent);

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in element.EnumerateObject())
                    {
                        var key = prop.Name;
                        var value = prop.Value;

                        if (value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                        {
                            sb.AppendLine($"{pad}{key}:");
                            sb.AppendLine(FormatJsonElement(value, indent + 2));
                        }
                        else
                        {
                            sb.AppendLine($"{pad}{key}: {FormatScalar(value)}");
                        }
                    }
                    break;
                case JsonValueKind.Array:
                    int idx = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        sb.AppendLine($"{pad}- Item {idx + 1}:");
                        sb.AppendLine(FormatJsonElement(item, indent + 2));
                        idx++;
                    }
                    break;
                default:
                    sb.AppendLine($"{pad}{FormatScalar(element)}");
                    break;
            }

            return sb.ToString().TrimEnd();
        }

        private string FormatScalar(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString() ?? string.Empty,
                JsonValueKind.Number => value.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => "null",
                _ => value.ToString()
            };
        }

        private void CopyIdButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_summary?.TournamentId))
                {
                    Clipboard.SetText(_summary.TournamentId);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error copying tournament ID: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
