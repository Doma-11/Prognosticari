namespace FootballPredictionAPI.DTOs.Predictions
{
    public class PredictionResponseDto
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public int PredictedHomeGoals { get; set; }
        public int PredictedAwayGoals { get; set; }
        public int? PointsAwarded { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
