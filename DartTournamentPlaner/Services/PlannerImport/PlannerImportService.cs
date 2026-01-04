using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DartTournamentPlaner.Helpers;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Services.PlannerImport
{
    public class PlannerImportService
    {
        private readonly TournamentManagementService _tournamentService;
        private readonly DataService _dataService;
        private readonly LocalizationService? _localizationService;

        public PlannerImportService(TournamentManagementService tournamentService, DataService dataService, LocalizationService? localizationService)
        {
            _tournamentService = tournamentService ?? throw new ArgumentNullException(nameof(tournamentService));
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _localizationService = localizationService;
        }

        public async Task<bool> ImportAsync(PlannerTournamentSummary summary, IEnumerable<PlannerParticipant> participants, Window? owner = null)
        {
            ArgumentNullException.ThrowIfNull(summary);
            ArgumentNullException.ThrowIfNull(participants);

            var participantList = participants.ToList();

            if (_tournamentService.HasActiveTournament())
            {
                var proceed = TournamentDialogHelper.ShowResetTournamentConfirmation(owner, _localizationService);
                if (!proceed)
                {
                    return false;
                }
            }

            var data = BuildTournamentData(summary, participantList);
            var targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tournament_data.json");
            _dataService.SaveTournamentToFile(data, targetPath);

            _tournamentService.ResetAllTournaments();
            await _tournamentService.LoadDataAsync();
            _tournamentService.TriggerUIRefresh();

            if (Application.Current?.MainWindow is DartTournamentPlaner.MainWindow mainWindow)
            {
                mainWindow.RefreshTournamentData();
            }

            return true;
        }

        private static TournamentData BuildTournamentData(PlannerTournamentSummary summary, List<PlannerParticipant> participants)
        {
            var data = new TournamentData
            {
                TournamentId = summary.TournamentId,
                TournamentName = summary.Name,
                TournamentDescription = summary.Description,
                TournamentLocation = summary.Location,
                TournamentStartTimeIso = summary.StartDate,
                TournamentTotalPlayers = summary.TotalPlayers ?? participants.Count,
                FeaturePublicView = true,
                FeaturePowerScoring = false,
                FeatureQrRegistration = false
            };

            var classNames = new List<string>();
            if (summary.Classes != null && summary.Classes.Any())
            {
                classNames.AddRange(summary.Classes.Select(c => c.Name).Where(n => !string.IsNullOrWhiteSpace(n))!);
            }

            // Always include default classes to satisfy existing UI expectations
            classNames.AddRange(new[] { "Platin", "Gold", "Silber", "Bronze" });

            var participantClassNames = participants.Select(p => p.ClassName).Where(n => !string.IsNullOrWhiteSpace(n));
            classNames.AddRange(participantClassNames!);
            classNames = classNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (classNames.Count == 0)
            {
                classNames.Add("Platin");
            }

            var classIdMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "Platin", 1 },
                { "Gold", 2 },
                { "Silber", 3 },
                { "Bronze", 4 }
            };

            var rulesByClass = summary.ParsedGameRules.ToDictionary(
                r => r.ClassName ?? string.Empty,
                r => r,
                StringComparer.OrdinalIgnoreCase);

            var classes = new List<TournamentClass>();
            int nextId = 1;
            int playerId = 1;
            var assignedParticipants = new HashSet<PlannerParticipant>();

            foreach (var className in classNames)
            {
                var tournamentClass = new TournamentClass();
                tournamentClass.Name = className;
                tournamentClass.Id = classIdMap.TryGetValue(className, out var knownId) ? knownId : nextId;
                nextId++;

                var ruleSet = rulesByClass.TryGetValue(className, out var classRule) ? classRule : null;
                tournamentClass.GameRules = MapGameRules(ruleSet, summary.GameMode);

                var classParticipants = participants
                    .Where(p => !assignedParticipants.Contains(p) && string.Equals(p.ClassName, className, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (classParticipants.Count == 0 && (summary.Classes?.Count ?? 0) == 1)
                {
                    classParticipants = participants.Where(p => !assignedParticipants.Contains(p)).ToList();
                }

                var displayNameMap = BuildDisplayNameMap(classParticipants);

                var groups = classParticipants
                    .GroupBy(p => string.IsNullOrWhiteSpace(p.GroupName) ? "Group 1" : p.GroupName!)
                    .Select((g, idx) =>
                    {
                        var group = new Group(idx + 1, g.Key ?? $"Group {idx + 1}");
                        foreach (var participant in g)
                        {
                            var name = displayNameMap.TryGetValue(participant, out var dn) ? dn : BuildPlayerName(participant);
                            group.Players.Add(new Player(playerId++, name)
                            {
                                FirstName = participant.FirstName,
                                LastName = participant.LastName,
                                Nickname = participant.Nickname,
                                Email = participant.Email
                            });
                            assignedParticipants.Add(participant);
                        }

                        return group;
                    })
                    .ToList();

                tournamentClass.Groups = new ObservableCollection<Group>(groups);
                tournamentClass.EnsureGroupPhaseExists();

                classes.Add(tournamentClass);
            }

            // Sort classes by Id to keep UI stable
            classes = classes.OrderBy(c => c.Id).ToList();
            data.TournamentClasses = classes;

            ApplyImportedMatches(summary, data);

            return data;
        }

        private static void ApplyImportedMatches(PlannerTournamentSummary summary, TournamentData data)
        {
            if (summary.Matches == null || summary.Matches.Count == 0)
            {
                return;
            }

            var classesById = data.TournamentClasses.ToDictionary(c => c.Id);
            var classesByName = data.TournamentClasses.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
            var nextPlayerId = data.TournamentClasses
                .SelectMany(c => c.Groups)
                .SelectMany(g => g.Players)
                .Select(p => p.Id)
                .DefaultIfEmpty(0)
                .Max() + 1;

            foreach (var plannerMatch in summary.Matches)
            {
                var tournamentClass = FindTournamentClass(plannerMatch, classesById, classesByName);
                if (tournamentClass == null)
                {
                    continue;
                }

                var group = FindGroup(tournamentClass, plannerMatch);
                if (group == null)
                {
                    continue;
                }

                var player1 = ResolvePlayer(group, plannerMatch.Player1) ?? CreatePlaceholderPlayer(group, plannerMatch.Player1, ref nextPlayerId);
                var player2 = ResolvePlayer(group, plannerMatch.Player2);

                if (player2 == null && !string.IsNullOrWhiteSpace(plannerMatch.Player2))
                {
                    player2 = CreatePlaceholderPlayer(group, plannerMatch.Player2, ref nextPlayerId);
                }

                var match = new Match
                {
                    Id = plannerMatch.MatchId != 0 ? plannerMatch.MatchId : group.Matches.Count + 1,
                    UniqueId = !string.IsNullOrWhiteSpace(plannerMatch.UniqueId) ? plannerMatch.UniqueId! : (!string.IsNullOrWhiteSpace(plannerMatch.Id) ? plannerMatch.Id! : Guid.NewGuid().ToString()),
                    Player1 = player1,
                    Player2 = player2,
                    Player1Sets = plannerMatch.Player1Sets,
                    Player2Sets = plannerMatch.Player2Sets,
                    Player1Legs = plannerMatch.Player1Legs,
                    Player2Legs = plannerMatch.Player2Legs,
                    Notes = string.IsNullOrWhiteSpace(plannerMatch.Notes) ? null : plannerMatch.Notes,
                    UsesSets = plannerMatch.GameRulesUsed?.PlayWithSets ?? plannerMatch.Player1Sets > 0 || plannerMatch.Player2Sets > 0,
                    CreatedAt = plannerMatch.CreatedAt ?? DateTime.Now,
                    StartTime = plannerMatch.StartedAt,
                    EndTime = plannerMatch.FinishedAt,
                    FinishedAt = plannerMatch.FinishedAt
                };

                match.Status = MapMatchStatus(plannerMatch.Status, player2 == null);

                var winner = ResolvePlayer(group, plannerMatch.Winner);
                if (winner == null && match.Status == MatchStatus.Bye)
                {
                    winner = player1;
                }

                match.Winner = winner;

                if (match.Winner == null && match.Status is MatchStatus.Finished or MatchStatus.Bye)
                {
                    match.DetermineWinner();
                }

                match.EnsureUniqueId();
                group.Matches.Add(match);
            }

            foreach (var group in data.TournamentClasses.SelectMany(c => c.Groups))
            {
                if (group.Matches.Count > 0)
                {
                    group.MatchesGenerated = true;
                }
            }
        }

        private static TournamentClass? FindTournamentClass(PlannerMatch plannerMatch, Dictionary<int, TournamentClass> classesById, Dictionary<string, TournamentClass> classesByName)
        {
            if (plannerMatch.ClassId.HasValue && classesById.TryGetValue(plannerMatch.ClassId.Value, out var byId))
            {
                return byId;
            }

            if (!string.IsNullOrWhiteSpace(plannerMatch.ClassName) && classesByName.TryGetValue(plannerMatch.ClassName, out var byName))
            {
                return byName;
            }

            return null;
        }

        private static Group? FindGroup(TournamentClass tournamentClass, PlannerMatch plannerMatch)
        {
            var groups = tournamentClass.Groups;

            if (plannerMatch.GroupId.HasValue)
            {
                var byId = groups.FirstOrDefault(g => g.Id == plannerMatch.GroupId.Value);
                if (byId != null)
                {
                    return byId;
                }
            }

            if (!string.IsNullOrWhiteSpace(plannerMatch.GroupName))
            {
                var byName = groups.FirstOrDefault(g => string.Equals(g.Name, plannerMatch.GroupName, StringComparison.OrdinalIgnoreCase));
                if (byName != null)
                {
                    return byName;
                }
            }

            return groups.FirstOrDefault();
        }

        private static Player? ResolvePlayer(Group group, string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var normalized = name.Trim();
            var player = group.Players.FirstOrDefault(p => string.Equals(p.Name, normalized, StringComparison.OrdinalIgnoreCase));
            if (player != null)
            {
                return player;
            }

            player = group.Players.FirstOrDefault(p => string.Equals(p.Nickname, normalized, StringComparison.OrdinalIgnoreCase));
            if (player != null)
            {
                return player;
            }

            player = group.Players.FirstOrDefault(p =>
                !string.IsNullOrWhiteSpace(p.FirstName) &&
                !string.IsNullOrWhiteSpace(p.LastName) &&
                string.Equals($"{p.FirstName} {p.LastName}", normalized, StringComparison.OrdinalIgnoreCase));

            return player;
        }

        private static Player CreatePlaceholderPlayer(Group group, string? name, ref int nextPlayerId)
        {
            var displayName = string.IsNullOrWhiteSpace(name) ? $"Player {nextPlayerId}" : name.Trim();
            var player = new Player(nextPlayerId++, displayName);
            group.Players.Add(player);
            return player;
        }

        private static MatchStatus MapMatchStatus(string? status, bool isBye)
        {
            if (isBye)
            {
                return MatchStatus.Bye;
            }

            return status?.ToLowerInvariant() switch
            {
                "inprogress" => MatchStatus.InProgress,
                "finished" => MatchStatus.Finished,
                "notstarted" => MatchStatus.NotStarted,
                _ => MatchStatus.NotStarted
            };
        }

        private static GameRules MapGameRules(PlannerClassRuleSet? ruleSet, string? gameMode)
        {
            var rules = new GameRules
            {
                GameMode = MapGameMode(ruleSet?.GameRules?.GameMode),
                FinishMode = MapFinishMode(ruleSet?.GameRules?.FinishMode),
                LegsToWin = ruleSet?.GameRules?.LegsToWin ?? 3,
                PlayWithSets = ruleSet?.GameRules?.PlayWithSets ?? false,
                SetsToWin = ruleSet?.GameRules?.SetsToWin ?? 3,
                LegsPerSet = ruleSet?.GameRules?.LegsPerSet ?? 3,
                QualifyingPlayersPerGroup = ruleSet?.GameRules?.QualifyingPlayersPerGroup ?? 2,
                KnockoutMode = MapKnockoutMode(ruleSet?.GameRules?.KnockoutMode),
                IncludeGroupPhaseLosersBracket = ruleSet?.GameRules?.IncludeGroupPhaseLosersBracket ?? false,
                SkipGroupPhase = ruleSet?.GameRules?.SkipGroupPhase ?? false,
                PostGroupPhaseMode = PostGroupPhaseMode.KnockoutBracket
            };

            if (string.Equals(gameMode, "round-robin", StringComparison.OrdinalIgnoreCase))
            {
                rules.PostGroupPhaseMode = PostGroupPhaseMode.RoundRobinFinals;
            }

            if (ruleSet?.GameRules?.KnockoutRoundRules?.Rules != null)
            {
                rules.KnockoutRoundRules.Clear();
                foreach (var kvp in ruleSet.GameRules.KnockoutRoundRules.Rules)
                {
                    if (Enum.TryParse<KnockoutRound>(kvp.Key, out var round))
                    {
                        rules.KnockoutRoundRules[round] = new RoundRules(kvp.Value.SetsToWin, kvp.Value.LegsToWin, kvp.Value.LegsPerSet);
                    }
                }
            }

            if (ruleSet?.GameRules?.RoundRobinFinalsRules?.Rules != null)
            {
                rules.RoundRobinFinalsRules.Clear();
                foreach (var kvp in ruleSet.GameRules.RoundRobinFinalsRules.Rules)
                {
                    if (Enum.TryParse<RoundRobinFinalsRound>(kvp.Key, out var round))
                    {
                        rules.RoundRobinFinalsRules[round] = new RoundRules(kvp.Value.SetsToWin, kvp.Value.LegsToWin, kvp.Value.LegsPerSet);
                    }
                }
            }

            return rules;
        }

        private static GameMode MapGameMode(int? mode)
        {
            return mode switch
            {
                0 => GameMode.Points501,
                1 => GameMode.Points401,
                2 => GameMode.Points301,
                _ => GameMode.Points501
            };
        }

        private static FinishMode MapFinishMode(int? mode)
        {
            return mode switch
            {
                1 => FinishMode.DoubleOut,
                _ => FinishMode.SingleOut
            };
        }

        private static KnockoutMode MapKnockoutMode(int? mode)
        {
            return mode switch
            {
                1 => KnockoutMode.DoubleElimination,
                _ => KnockoutMode.SingleElimination
            };
        }

        private static string BuildPlayerName(PlannerParticipant participant)
        {
            var first = participant.FirstName ?? string.Empty;
            var last = participant.LastName ?? string.Empty;
            var combined = string.Join(" ", new[] { first, last }.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (!string.IsNullOrWhiteSpace(combined)) return combined;
            if (!string.IsNullOrWhiteSpace(participant.Nickname)) return participant.Nickname!;
            if (!string.IsNullOrWhiteSpace(participant.Name)) return participant.Name!;
            return participant.Email ?? "";
        }

        private static Dictionary<PlannerParticipant, string> BuildDisplayNameMap(List<PlannerParticipant> participants)
        {
            var map = new Dictionary<PlannerParticipant, string>();

            // 1. Teilnehmer mit Spitznamen übernehmen
            foreach (var p in participants.Where(p => !string.IsNullOrWhiteSpace(p.Nickname)))
            {
                map[p] = p.Nickname!.Trim();
            }

            // 2. Teilnehmer ohne Spitznamen nach Vornamen gruppieren
            var noNick = participants.Where(p => string.IsNullOrWhiteSpace(p.Nickname)).ToList();
            var groups = noNick.GroupBy(p => GetBaseFirstName(p), StringComparer.OrdinalIgnoreCase);

            foreach (var group in groups)
            {
                var firstName = string.IsNullOrWhiteSpace(group.Key) ? "Player" : group.Key;
                var list = group.ToList();

                if (list.Count == 1)
                {
                    map[list[0]] = firstName;
                    continue;
                }

                int suffixLen = 1;
                int maxLastLen = list.Max(p => (p.LastName ?? string.Empty).Trim().Length);

                while (true)
                {
                    var candidates = new Dictionary<string, List<PlannerParticipant>>(StringComparer.OrdinalIgnoreCase);

                    foreach (var p in list)
                    {
                        var last = (p.LastName ?? string.Empty).Trim();
                        var suffix = string.Empty;
                        if (!string.IsNullOrEmpty(last))
                        {
                            var take = Math.Min(suffixLen, last.Length);
                            suffix = " " + last.Substring(0, take);
                        }

                        var candidate = (firstName + suffix).Trim();
                        if (!candidates.TryGetValue(candidate, out var bucket))
                        {
                            bucket = new List<PlannerParticipant>();
                            candidates[candidate] = bucket;
                        }
                        bucket.Add(p);
                    }

                    if (candidates.All(c => c.Value.Count == 1))
                    {
                        foreach (var kv in candidates)
                        {
                            map[kv.Value[0]] = kv.Key;
                        }
                        break;
                    }

                    if (suffixLen >= maxLastLen)
                    {
                        foreach (var kv in candidates)
                        {
                            if (kv.Value.Count == 1)
                            {
                                map[kv.Value[0]] = kv.Key;
                            }
                            else
                            {
                                for (int i = 0; i < kv.Value.Count; i++)
                                {
                                    map[kv.Value[i]] = $"{kv.Key} #{i + 1}";
                                }
                            }
                        }
                        break;
                    }

                    suffixLen++;
                }
            }

            return map;
        }

        private static string GetBaseFirstName(PlannerParticipant participant)
        {
            var first = participant.FirstName;
            if (!string.IsNullOrWhiteSpace(first)) return first.Trim();
            if (!string.IsNullOrWhiteSpace(participant.Name)) return participant.Name!.Trim();
            if (!string.IsNullOrWhiteSpace(participant.Email)) return participant.Email!.Trim();
            return "Player";
        }
    }
}
