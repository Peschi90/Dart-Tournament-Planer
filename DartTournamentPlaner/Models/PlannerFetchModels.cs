using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DartTournamentPlaner.Models
{
    public class PlannerFetchTournamentsRequest
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "planner-fetch-tournaments";

        [JsonPropertyName("licenseKey")]
        public string LicenseKey { get; set; } = string.Empty;

        [JsonPropertyName("days")]
        public int Days { get; set; } = 14;
    }

    public class PlannerTournamentSummary
    {
        [JsonPropertyName("tournament_id")]
        public string? TournamentId { get; set; }

        [JsonPropertyName("license_key")]
        public string? LicenseKey { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("start_date")]
        public string? StartDate { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("game_mode")]
        public string? GameMode { get; set; }

        [JsonPropertyName("updated_at")]
        public string? UpdatedAt { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("game_rules")]
        public object? GameRules { get; set; }
    }

    public class PlannerTournamentsResponse
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "planner-tournaments-data";

        [JsonPropertyName("licenseKey")]
        public string? LicenseKey { get; set; }

        [JsonPropertyName("days")]
        public int Days { get; set; }

        [JsonPropertyName("tournaments")]
        public List<PlannerTournamentSummary> Tournaments { get; set; } = new();
    }

    public class PlannerFetchErrorResponse
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "planner-fetch-error";

        [JsonPropertyName("error")]
        public string Error { get; set; } = string.Empty;
    }
}
