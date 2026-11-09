namespace FootballPredictionAPI.DTOs.Tournaments
{
    public class JoinTournamentResponseDto
    {
        public int TournamentId { get; set; }
        public string TournamentName { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}
