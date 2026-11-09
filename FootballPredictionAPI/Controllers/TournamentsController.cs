using FootballPredictionAPI.DTOs.Tournaments;
using FootballPredictionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FootballPredictionAPI.Controllers
{
    [ApiController]
    [Route("api/tournaments")]
    [Authorize]
    public class TournamentsController : ControllerBase
    {

        private readonly ITournamentService _tournamentService;

        public TournamentsController(ITournamentService tournamentService)
        {
            _tournamentService = tournamentService;
        }


        //User represents the authenticated user making the HTTP request.
        private int GetCurrentUserId()
        {
            //FindFirstValue(ClaimTypes.NameIdentifier) reads the user's ID from their JWT claims.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int id))
                throw new UnauthorizedAccessException("Invalid user token.");

            return id;
        }

        // 1. CREATE TOURNAMENT
        // POST /api/tournaments

        [HttpPost]
        //[FromBody] tells ASP.NET Core to deserialize the JSON request into CreateTournamentRequestDto
        public async Task<ActionResult<CreatedTournamentResponseDto>> CreateTournament([FromBody] CreateTournamentRequestDto request)
        {
            int currentUserId= GetCurrentUserId();

            var result = await _tournamentService.CreateTournamentAsync(request, currentUserId);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        // 2. GET TOURNAMENT BY ID
        // GET /api/tournaments/5

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TournamentResponseDto>> GetTournamentById(int id)
        {
            int currentUserId = GetCurrentUserId();

            var tournament = await _tournamentService.GetTournamentByIdAsync(id, currentUserId);

            if(tournament== null)
            {
                return NotFound();
            }
            else
            {
                return Ok(tournament);
            }
        }


        // 3. GET MY TOURNAMENTS
        // GET /api/tournaments
        [HttpGet]
        public async Task<ActionResult<List<TournamentResponseDto>>> GetMyTournaments()
        {
            int currentUSerId = GetCurrentUserId();

            var tournaments = await _tournamentService.GetMyTournamentsAsync(currentUSerId);

            return Ok(tournaments);
        }


        // 4. JOIN TOURNAMENT
        // POST /api/tournaments/join
        [HttpPost("join")]
        public async Task<ActionResult<JoinTournamentResponseDto>> JoinTournament([FromBody] JoinTournamentRequestDto request)
        {
            int currentUserId = GetCurrentUserId();

            var result = await _tournamentService.JoinTournamentAsync(
                request, currentUserId);

            return Ok(result);
        }


        // 5. GET TOURNAMENT PARTICIPANTS
        // GET /api/tournaments/5/participants
        [HttpGet("{id:int}/participants")]
        public async Task<ActionResult<List<TournamentParticipantResponseDto>>> GetParticipants(
            int id)
        {
            int currentUserId = GetCurrentUserId();

            var participants = await _tournamentService.GetParticipantsAsync(
                id, currentUserId);

            return Ok(participants);
        }


        // 6. UPDATE TOURNAMENT
        // PUT /api/tournaments/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<TournamentResponseDto>> UpdateTournament(
            int id,
            [FromBody] UpdateTournamentRequestDto request)
        {
            int currentUserId = GetCurrentUserId();

            var result = await _tournamentService.UpdateTournamentAsync(
                id, request, currentUserId);

            return Ok(result);
        }


        // 7. LEAVE TOURNAMENT
        // POST /api/tournaments/5/leave
        [HttpPost("{id:int}/leave")]
        public async Task<IActionResult> LeaveTournament(int id)
        {
            int currentUserId = GetCurrentUserId();

            await _tournamentService.LeaveTournamentAsync(
                id, currentUserId);

            return NoContent();
        }
    }
}
