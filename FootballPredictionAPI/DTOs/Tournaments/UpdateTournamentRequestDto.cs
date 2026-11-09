namespace FootballPredictionAPI.DTOs.Tournaments
{
    public class UpdateTournamentRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
