using System.Collections.Generic;
using System.Text.Json;
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

    public class PlannerParticipant
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        // Legacy underscore payloads
        [JsonPropertyName("first_name")]
        public string? FirstNameLegacy { set => FirstName = value; }

        [JsonPropertyName("nickname")]
        public string? Nickname { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        // Legacy underscore payloads
        [JsonPropertyName("last_name")]
        public string? LastNameLegacy { set => LastName = value; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("className")]
        public string? ClassName { get; set; }

        // Legacy underscore payloads
        [JsonPropertyName("class_name")]
        public string? ClassNameLegacy { set => ClassName = value; }

        [JsonPropertyName("groupName")]
        public string? GroupName { get; set; }

        // Legacy underscore payloads
        [JsonPropertyName("group_name")]
        public string? GroupNameLegacy { set => GroupName = value; }
    }

    public class PlannerClassInfo
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("playerCount")]
        public int PlayerCount { get; set; }

        // Legacy underscore payloads
        [JsonPropertyName("player_count")]
        public int PlayerCountLegacy { set => PlayerCount = value; }
    }

    public sealed class PlannerClassInfoConverter : JsonConverter<PlannerClassInfo>
    {
        public override PlannerClassInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var name = reader.GetString();
                return new PlannerClassInfo { Name = name };
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                reader.Skip();
                return null;
            }

            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            var info = new PlannerClassInfo
            {
                Name = root.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : root.TryGetProperty("ClassName", out var cname) ? cname.GetString() : null,
                PlayerCount = root.TryGetProperty("playerCount", out var pcEl) && pcEl.TryGetInt32(out var pc) ? pc : 0
            };

            if (info.PlayerCount == 0 && root.TryGetProperty("player_count", out var pcLegacy) && pcLegacy.TryGetInt32(out var pcL))
            {
                info.PlayerCount = pcL;
            }

            return info;
        }

        public override void Write(Utf8JsonWriter writer, PlannerClassInfo value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("name", value.Name);
            writer.WriteNumber("playerCount", value.PlayerCount);
            writer.WriteEndObject();
        }
    }

    public class PlannerRoundRule
    {
        [JsonPropertyName("SetsToWin")]
        public int SetsToWin { get; set; }

        [JsonPropertyName("LegsToWin")]
        public int LegsToWin { get; set; }

        [JsonPropertyName("LegsPerSet")]
        public int LegsPerSet { get; set; }
    }

    public class PlannerRoundRuleContainer
    {
        [JsonPropertyName("Rules")]
        public Dictionary<string, PlannerRoundRule>? Rules { get; set; }
    }

    public class PlannerClassGameRules
    {
        [JsonPropertyName("GameMode")]
        public int GameMode { get; set; }

        [JsonPropertyName("FinishMode")]
        public int FinishMode { get; set; }

        [JsonPropertyName("LegsToWin")]
        public int LegsToWin { get; set; }

        [JsonPropertyName("PlayWithSets")]
        public bool PlayWithSets { get; set; }

        [JsonPropertyName("SetsToWin")]
        public int SetsToWin { get; set; }

        [JsonPropertyName("LegsPerSet")]
        public int LegsPerSet { get; set; }

        [JsonPropertyName("QualifyingPlayersPerGroup")]
        public int QualifyingPlayersPerGroup { get; set; }

        [JsonPropertyName("KnockoutMode")]
        public int KnockoutMode { get; set; }

        [JsonPropertyName("IncludeGroupPhaseLosersBracket")]
        public bool IncludeGroupPhaseLosersBracket { get; set; }

        [JsonPropertyName("SkipGroupPhase")]
        public bool SkipGroupPhase { get; set; }

        [JsonPropertyName("KnockoutRoundRules")]
        public PlannerRoundRuleContainer? KnockoutRoundRules { get; set; }

        [JsonPropertyName("RoundRobinFinalsRules")]
        public PlannerRoundRuleContainer? RoundRobinFinalsRules { get; set; }
    }

    public class PlannerClassRuleSet
    {
        [JsonPropertyName("ClassId")]
        public int ClassId { get; set; }

        [JsonPropertyName("ClassName")]
        public string? ClassName { get; set; }

        [JsonPropertyName("GameRules")]
        public PlannerClassGameRules? GameRules { get; set; }
    }

    public class PlannerMatchGameRules
    {
        [JsonPropertyName("gameMode")]
        public string? GameMode { get; set; }

        [JsonPropertyName("finishMode")]
        public string? FinishMode { get; set; }

        [JsonPropertyName("playWithSets")]
        public bool? PlayWithSets { get; set; }

        [JsonPropertyName("setsToWin")]
        public int? SetsToWin { get; set; }

        [JsonPropertyName("legsToWin")]
        public int? LegsToWin { get; set; }

        [JsonPropertyName("legsPerSet")]
        public int? LegsPerSet { get; set; }
    }

    public class PlannerMatch
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("matchId")]
        public int MatchId { get; set; }

        [JsonPropertyName("uniqueId")]
        public string? UniqueId { get; set; }

        [JsonPropertyName("hubIdentifier")]
        public string? HubIdentifier { get; set; }

        [JsonPropertyName("player1")]
        public string? Player1 { get; set; }

        [JsonPropertyName("player2")]
        public string? Player2 { get; set; }

        [JsonPropertyName("player1Sets")]
        public int Player1Sets { get; set; }

        [JsonPropertyName("player2Sets")]
        public int Player2Sets { get; set; }

        [JsonPropertyName("player1Legs")]
        public int Player1Legs { get; set; }

        [JsonPropertyName("player2Legs")]
        public int Player2Legs { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("winner")]
        public string? Winner { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("classId")]
        public int? ClassId { get; set; }

        [JsonPropertyName("className")]
        public string? ClassName { get; set; }

        [JsonPropertyName("groupId")]
        public int? GroupId { get; set; }

        [JsonPropertyName("groupName")]
        public string? GroupName { get; set; }

        [JsonPropertyName("matchType")]
        public string? MatchType { get; set; }

        [JsonPropertyName("gameRulesId")]
        public int? GameRulesId { get; set; }

        [JsonPropertyName("gameRulesUsed")]
        public PlannerMatchGameRules? GameRulesUsed { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("startedAt")]
        public DateTime? StartedAt { get; set; }

        [JsonPropertyName("finishedAt")]
        public DateTime? FinishedAt { get; set; }

        [JsonPropertyName("syncedAt")]
        public DateTime? SyncedAt { get; set; }
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

        [JsonPropertyName("classes")]
        public List<PlannerClassInfo>? Classes { get; set; }

        [JsonPropertyName("updated_at")]
        public string? UpdatedAt { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("game_rules")]
        public object? GameRules { get; set; }

        [JsonIgnore]
        public List<PlannerClassRuleSet> ParsedGameRules
        {
            get
            {
                try
                {
                    if (GameRules == null) return new List<PlannerClassRuleSet>();
                    var json = System.Text.Json.JsonSerializer.Serialize(GameRules);
                    return System.Text.Json.JsonSerializer.Deserialize<List<PlannerClassRuleSet>>(json, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<PlannerClassRuleSet>();
                }
                catch
                {
                    return new List<PlannerClassRuleSet>();
                }
            }
        }

        [JsonPropertyName("participants")]
        public List<PlannerParticipant>? Participants { get; set; }

        [JsonPropertyName("totalPlayers")]
        public int? TotalPlayers { get; set; }

        [JsonPropertyName("matches")]
        public List<PlannerMatch>? Matches { get; set; }
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
