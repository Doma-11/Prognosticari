using FootballPredictionAPI.DTOs.Predictions;
using FootballPredictionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FootballPredictionAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/tournaments/{tournamentId}/predictions")]
    public class PredictionsController:ControllerBase
    {
        private readonly IPredictionService _predictionService;

        public PredictionsController(IPredictionService predictionService)
        {
            _predictionService = predictionService;
        }



        [HttpPost]
        public async Task<ActionResult<PredictionResponseDto>> CreatePrediction(int tournamentId,[FromBody] CreatePredictionDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var prediction = await _predictionService.CreatePredictionAsync(tournamentId, userId, dto);

            return StatusCode(201, prediction);
        }

        [HttpPut("{predictionId}")]
        public async Task<ActionResult<PredictionResponseDto>> UpdatePrediction(int tournamentId,int predictionId,[FromBody] UpdatePredictionDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var prediction = await _predictionService.UpdatePredictionAsync(tournamentId, predictionId, userId, dto);

               

            return Ok(prediction);
        }

        [HttpGet("me")]
        public async Task<ActionResult<List<PredictionResponseDto>>> GetMyPredictions(int tournamentId)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var predictions = await _predictionService.GetMyPredictionsAsync(
                tournamentId,
                userId);

            return Ok(predictions);
        }

        [HttpGet("me/{matchId}")]
        public async Task<IActionResult> GetMyPredictionForMatch(int tournamentId, int matchId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var prediction= await _predictionService.GetMyPredictionAsync(tournamentId, matchId, userId);

            return Ok(prediction);
        }

        [HttpGet("/api/tournaments/{tournamentId}/matches")]
        public async Task<ActionResult<List<TournamentMatchDto>>> GetTournamentMatches(int tournamentId)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var matches = await _predictionService.GetTournamentMatchesAsync(
                tournamentId,
                userId);

            return Ok(matches);
        }

    }
}
