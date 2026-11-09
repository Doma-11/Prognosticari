namespace FootballPredictionAPI.DTOs.Tournaments
{
    public class CreateTournamentRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public int SeasonId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
