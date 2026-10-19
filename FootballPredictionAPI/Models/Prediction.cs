namespace FootballPredictionAPI.Models
{
    public class Prediction
    {
        public int Id { get; set; }
        public int TournamentParticipantId { get; set; }
        public int MatchId { get; set; }
        public int PredictedHomeGoals { get; set; }
        public int PredictedAwayGoals { get; set; }
        public int? PointsAwarded { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public TournamentParticipant TournamentParticipant { get; set; } = null!;

        public Match Match { get; set; } = null!;
        

    }
}
