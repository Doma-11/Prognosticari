namespace FootballPredictionAPI.Models
{
    public class Season
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public int LeagueId { get; set; }

        public League League { get; set; } = null!;

        public ICollection<Match> Matches { get; set; } = new List<Match>();

        public ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
    }
}
