namespace FootballPredictionAPI.Services.Interfaces
{
    public interface IScoringService
    {
        //Task- Perform an asynchronous operation without returning a value
        Task ScoreMatchAsync(int matchId);
    }
}
