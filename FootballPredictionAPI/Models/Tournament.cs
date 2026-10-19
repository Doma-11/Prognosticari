namespace FootballPredictionAPI.Models
{
    public class Tournament
    {
        public int Id { get; set; }

        public string Name { get; set; }= string.Empty;
        public int SeasonId { get; set;}

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public string InvitationCode { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; }
        public ICollection<TournamentParticipant> Participants { get; set; } = new List<TournamentParticipant>();

        public Season Season { get; set; } = null!;

        public User CreatedByUser { get; set; } = null!;
    }
}
