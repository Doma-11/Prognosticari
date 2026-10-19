namespace FootballPredictionAPI.Models
{
    public class TournamentParticipant
    {
        public int Id { get; set; }
        
        public int TournamentId { get; set; }
        public int UserId { get; set; }
        public int TotalPoints { get; set; }
        public DateTime JoinedAt { get; set; }

        public User User { get; set; } = null!;

        public Tournament Tournament { get; set; } = null!;
        public ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();
    }
}
