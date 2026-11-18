using FootballPredictionAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballPredictionAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin/football")]
    public class FootballAdminController : ControllerBase
    {
        private readonly FootballApiService _footballApiService;
        private readonly FootballSyncService _footballSyncService;

        public FootballAdminController(FootballApiService footballApiService, FootballSyncService footballSyncService)
        {
            _footballApiService = footballApiService;
            _footballSyncService = footballSyncService;
        }

        [HttpGet("test-competitions")]
        public async Task<IActionResult> TestCompetions()
        {
            var competitions = await _footballApiService.GetCompetitionsAsync();

            return Ok(competitions);
        }

        [HttpPost("sync-leagues")]
        public async Task<IActionResult> SyncLeagues()
        {
            var importedCount= await _footballSyncService.SyncLeagueAsync();

            return Ok(new
            {
                Message = "League sync completed.",
                ImportedCount = importedCount

            });
        }

        [HttpPost("sync-seasons")]
        public async Task<IActionResult> SyncSeasons()
        {
            var importedCount = await _footballSyncService.SyncSeasonsAsync();

            return Ok(new
            {
                Message = "Season synchronization completed.",
                ImportedCount = importedCount
            });
        }

        [HttpGet("test-teams/{competitionCode}")]
        public async Task<IActionResult> TestTeams(string competitionCode)
        {
            var teams = await _footballApiService.GetTeamsAsync(competitionCode);

            return Ok(teams);
        }


        [HttpPost("sync-teams/{competitionCode}")]
        public async Task<IActionResult> SyncTeams(string competitionCode)
        {
            var importedCount = await _footballSyncService
                .SyncTeamsAsync(competitionCode);

            return Ok(new
            {
                Message = "Team synchronization completed.",
                ImportedCount = importedCount
            });
        }

        [HttpPost("sync-all-teams")]
        public async Task<IActionResult> SyncAllTeams()
        {
            var results = await _footballSyncService.SyncAllTeamsAsync();

            return Ok(new
            {
                Message = "Bulk team synchronization completed.",
                Results = results
            });
        }

        [HttpGet("test-matches/{competitionCode}")]
        public async Task<IActionResult> TestMatches(string competitionCode)
        {
            var matches = await _footballApiService
                .GetMatchesAsync(competitionCode);

            return Ok(matches);
        }

        [HttpPost("sync-matches/{competitionCode}")]
        public async Task<IActionResult> SyncMatches(string competitionCode)
        {
            var importedCount = await _footballSyncService
                .SyncMatchesAsync(competitionCode);

            return Ok(new
            {
                Message = "Match synchronization completed.",
                ImportedCount = importedCount
            });
        }

        [HttpPost("sync-all-matches")]
        public async Task<IActionResult> SyncAllMatches()
        {
            var results = await _footballSyncService.SyncAllMatchesAsync();

            return Ok(new
            {
                Message = "Bulk match synchronization completed.",
                Results = results
            });
        }

    }
}
