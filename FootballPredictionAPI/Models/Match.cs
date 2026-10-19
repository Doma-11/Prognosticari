using FootballPredictionAPI.Enums;

namespace FootballPredictionAPI.Models
{
    public class Match
    {
        public int Id { get; set; }
        public int SeasonId { get; set; }
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public DateTime MatchDate { get; set; }
        public int? HomeTeamScore { get; set; }
        public int? AwayTeamScore { get; set; }
        public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
        public int ExternalApiId { get; set; }
        public Season Season { get; set; } =  null!;
        public ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();

        public Team HomeTeam { get; set; } = null!;
        public Team AwayTeam { get; set; } = null!;
    }

    
}
