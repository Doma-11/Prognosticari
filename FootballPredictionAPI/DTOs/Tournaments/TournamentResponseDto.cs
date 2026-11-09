namespace FootballPredictionAPI.DTOs.Tournaments
{
    public class TournamentResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SeasonId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int CreatedByUserId { get; set; }
        public int ParticipantCount { get; set; }
    }
}
