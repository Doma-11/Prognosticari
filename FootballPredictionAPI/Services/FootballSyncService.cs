using FootballPredictionAPI.Data;
using FootballPredictionAPI.DTOs.FootballApi;
using FootballPredictionAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FootballPredictionAPI.Enums;

namespace FootballPredictionAPI.Services
{
    public class FootballSyncService
    {
        private readonly FootballApiService _footballApiService;
        private readonly AppDbContext _context;
        private readonly ILogger<FootballSyncService> _logger;

        public FootballSyncService(FootballApiService footballApiService,AppDbContext context,ILogger<FootballSyncService> logger)
        {
            _footballApiService = footballApiService;
            _context = context;
            _logger = logger;
        }


        public async Task<int> SyncLeagueAsync()
        {
            var response = await _footballApiService.GetCompetitionsAsync();

            int importedCount = 0;


            //EF Core searches PostgreSQL for a league with the same external API ID.
            foreach (var competition in response.Competitions)
            {
                var existingLeague = await _context.Leagues
             .FirstOrDefaultAsync(l => l.ExternalApiId == competition.Id);


                if (existingLeague != null)
                {
                    continue;
                }

                var league = MapCompetitionToLeague(competition);

                _context.Leagues.Add(league);
                importedCount++;
            }
            await _context.SaveChangesAsync();
            return importedCount;
        }

        private League MapCompetitionToLeague(FootballCompetitionDto competition)
        {
            return new League
            {
                Name = competition.Name,
                LogoUrl = competition.Emblem ?? string.Empty,
                ExternalApiId = competition.Id
            };
        }

        private Season MapFootballSeasonToSeason(FootballSeasonDto footballSeason,int leagueId)
        {

            int startYear = footballSeason.StartDate.Year;
            int endYear = footballSeason.EndDate.Year;


            //ternary conditional operator: "condition ? valueIfTrue : valueIfFalse"
            string seasonName = startYear == endYear ? startYear.ToString() : $"{startYear}/{endYear}";

            return new Season
            {
                Name = $"{footballSeason.StartDate.Year}/{footballSeason.EndDate.Year}",
                StartDate = footballSeason.StartDate,
                EndDate = footballSeason.EndDate,
                LeagueId = leagueId,
                ExternalApiId = footballSeason.Id
            };
        }


        //fetches competitions, finds their corresponding leagues, and imports their current seasons.
        public async Task<int> SyncSeasonsAsync()
        {
            var response = await _footballApiService.GetCompetitionsAsync();

            int importedCount = 0;

            foreach (var competition in response.Competitions)
            {
                if (competition.CurrentSeason == null)
                {
                    continue;
                }

                var league = await _context.Leagues
                    .FirstOrDefaultAsync(l => l.ExternalApiId == competition.Id);

                if (league == null)
                {
                    continue;
                }

                var season = MapFootballSeasonToSeason(
                    competition.CurrentSeason,
                    league.Id);

                var existingSeason = await _context.Seasons.FirstOrDefaultAsync(s =>
                 s.ExternalApiId == competition.CurrentSeason.Id);

                if (existingSeason == null)
                {
                    existingSeason = await _context.Seasons.FirstOrDefaultAsync(s =>
                        s.LeagueId == league.Id &&
                        s.ExternalApiId == null &&
                        s.StartDate == competition.CurrentSeason.StartDate &&
                        s.EndDate == competition.CurrentSeason.EndDate);
                }

                if (existingSeason != null)
                {
                    existingSeason.ExternalApiId = competition.CurrentSeason.Id;
                    continue;
                }

                _context.Seasons.Add(season);
                importedCount++;
            }

            await _context.SaveChangesAsync();

            return importedCount;
        }

        private Team MapFootballTeamToTeam(FootballTeamDto footballTeam)
        {
            return new Team
            {
                Name = footballTeam.Name,
                LogoUrl = footballTeam.Crest ?? string.Empty,
                ExternalApiId = footballTeam.Id
            };
        }



        public async Task<int> SyncTeamsAsync(string competitionCode)
        {
            var response = await _footballApiService.GetTeamsAsync(competitionCode);

            int importedCount = 0;

            foreach (var footballTeam in response.Teams)
            {
                bool teamExists = await _context.Teams
                    .AnyAsync(t => t.ExternalApiId == footballTeam.Id);

                if (teamExists)
                {
                    continue;
                }

                var team = MapFootballTeamToTeam(footballTeam);

                _context.Teams.Add(team);
                importedCount++;
            }

            await _context.SaveChangesAsync();

            return importedCount;
        }

        public async Task<Dictionary<string, int>> SyncAllTeamsAsync()
        {
            var results = new Dictionary<string, int>();

            foreach (var competitionCode in CompetitionCodes)
            {
                try
                {
                    int importedCount = await SyncTeamsAsync(competitionCode);

                    results.Add(competitionCode, importedCount);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to synchronize teams for competition {CompetitionCode}",
                        competitionCode);

                    results.Add(competitionCode, -1);
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogError(
                        ex,
                        "Match synchronization timed out for competition {CompetitionCode}",
                        competitionCode);

                    results.Add(competitionCode, -1);
                }

                if (competitionCode != CompetitionCodes[^1])
                {
                    await Task.Delay(TimeSpan.FromSeconds(7));
                }
            }

            return results;
        }



        private static readonly string[] CompetitionCodes ={
                        "PL",   // Premier League
                        "PD",   // La Liga
                        "BL1",  // Bundesliga
                        "SA",   // Serie A
                        "FL1",  // Ligue 1
                        "CL",   // Champions League
                        "EC",   // European Championship
                        "WC",   // World Cup
                        "DED",  // Eredivisie
        };
        private MatchStatus MapMatchStatus(string apiStatus)
        {
            return apiStatus switch
            {
                "SCHEDULED" or "TIMED" => MatchStatus.Scheduled,
                "IN_PLAY" or "PAUSED" => MatchStatus.Live,
                "FINISHED" => MatchStatus.Finished,
                "POSTPONED" or "SUSPENDED" => MatchStatus.Postponed,
                "CANCELLED" => MatchStatus.Cancelled,

                _ => throw new ArgumentException(
                    $"Unknown football API match status: {apiStatus}")
            };
        }

        private async Task<Season?> FindSeasonAsync(FootballMatchDto footballMatch)
        {
            if (footballMatch.Season == null)
            {
                return null;
            }

            return await _context.Seasons.FirstOrDefaultAsync(s =>
                s.ExternalApiId == footballMatch.Season.Id);
        }

        private async Task<Team?> FindTeamAsync(FootballTeamDto? footballTeam)
        {
            if (footballTeam == null || footballTeam.Id <= 0)
            {
                return null;
            }

            return await _context.Teams
                .FirstOrDefaultAsync(t =>
                    t.ExternalApiId == footballTeam.Id);
        }

        private Match MapFootballMatchToMatch(FootballMatchDto footballMatch, Season season, Team homeTeam, Team awayTeam)
        {
            return new Match
            {
                ExternalApiId = footballMatch.Id,
                SeasonId = season.Id,
                HomeTeamId = homeTeam.Id,
                AwayTeamId = awayTeam.Id,
                MatchDate = footballMatch.UtcDate,
                Status = MapMatchStatus(footballMatch.Status),
                HomeTeamScore = footballMatch.Score?.FullTime?.Home,
                AwayTeamScore = footballMatch.Score?.FullTime?.Away
            };
        }

        public async Task<int> SyncMatchesAsync(string competitionCode)
        {
            var response = await _footballApiService.GetMatchesAsync(competitionCode);

            var matchApiIds = response.Matches
                .Select(m => m.Id)
                .Distinct()
                .ToList();

            var existingMatches = await _context.Matches
                    .Where(m => matchApiIds.Contains(m.ExternalApiId))
                    .ToDictionaryAsync(m => m.ExternalApiId);




            //SelectMany() combines the home and away team IDs into one sequence. Where() removes missing IDs, and Distinct() removes duplicates.
            var teamApiIds = response.Matches
            .SelectMany(m => new[]
            {
                m.HomeTeam?.Id,
                m.AwayTeam?.Id
            })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

            var teamsByApiId = await _context.Teams
            .Where(t => teamApiIds.Contains(t.ExternalApiId))
            .ToDictionaryAsync(t => t.ExternalApiId);

            int importedCount = 0;


            var seasonApiIds= response.Matches
                .Where(m=>m.Season != null)
                .Select(m=>m.Season!.Id)
                .Distinct()
                .ToList();

            var seasonsByApiId = await _context.Seasons
                .Where(s => s.ExternalApiId.HasValue && seasonApiIds.Contains(s.ExternalApiId.Value))
                .ToDictionaryAsync(s => s.ExternalApiId!.Value);


            foreach (var footballMatch in response.Matches)
            {
                Season? season = null;

                if (footballMatch.Season != null)
                {
                    seasonsByApiId.TryGetValue(footballMatch.Season.Id, out season);
                }
                Team? homeTeam = null;
                Team? awayTeam = null;

                if (footballMatch.HomeTeam != null)
                {
                    teamsByApiId.TryGetValue(footballMatch.HomeTeam.Id, out homeTeam);
                }

                if (footballMatch.AwayTeam != null)
                {
                    teamsByApiId.TryGetValue(footballMatch.AwayTeam.Id, out awayTeam);
                }

                if (season == null || homeTeam == null || awayTeam == null)
                {
                    continue;
                }

                //TryGetValue= It searches the dictionary for footballMatch.Id,
                //If the key exists, existingMatch contains the corresponding tracked Match entity.If the key doesn't exist, existingMatch is null

                existingMatches.TryGetValue(footballMatch.Id, out var existingMatch);

                if (existingMatch != null)
                {
                    existingMatch.MatchDate = footballMatch.UtcDate;
                    existingMatch.Status = MapMatchStatus(footballMatch.Status);
                    existingMatch.HomeTeamScore = footballMatch.Score?.FullTime?.Home;
                    existingMatch.AwayTeamScore = footballMatch.Score?.FullTime?.Away;

                    continue;
                }
                var match = MapFootballMatchToMatch(footballMatch,season,homeTeam,awayTeam);

                _context.Matches.Add(match);
                existingMatches.Add(footballMatch.Id, match);
                importedCount++;
            }
            await _context.SaveChangesAsync();

            return importedCount;
        }

        public async Task<Dictionary<string, int>> SyncAllMatchesAsync()
        {
            var results = new Dictionary<string, int>();

            foreach (var competitionCode in CompetitionCodes)
            {
                try
                {
                    int importedCount = await SyncMatchesAsync(competitionCode);

                    results.Add(competitionCode, importedCount);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to synchronize matches for competition {CompetitionCode}",
                        competitionCode);

                    results.Add(competitionCode, -1);
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogError(
                        ex,
                        "Match synchronization timed out for competition {CompetitionCode}",
                        competitionCode);

                    results.Add(competitionCode, -1);
                }

                if (competitionCode != CompetitionCodes[^1])
                {
                    await Task.Delay(TimeSpan.FromSeconds(7));
                }
            }

            return results;
        }
    }
}
