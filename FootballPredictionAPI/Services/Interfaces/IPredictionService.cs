using FootballPredictionAPI.DTOs.Predictions;

namespace FootballPredictionAPI.Services.Interfaces
{
    public interface IPredictionService
    {
        Task<PredictionResponseDto> CreatePredictionAsync(int tournamentId, int userId, CreatePredictionDto createPredictionDto);
        Task<PredictionResponseDto> UpdatePredictionAsync(int tournamentId,int predictionId,int userId,UpdatePredictionDto dto);
        Task<List<PredictionResponseDto>> GetMyPredictionsAsync(int tournamentId, int userId);
        Task<PredictionResponseDto> GetMyPredictionAsync(int tournamentId, int matchId, int userId);
        Task<List<TournamentMatchDto>> GetTournamentMatchesAsync(int tournamentId,int userId);
        
    }
}
