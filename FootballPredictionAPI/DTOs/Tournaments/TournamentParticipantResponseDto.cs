namespace FootballPredictionAPI.DTOs.Tournaments
{
    public class TournamentParticipantResponseDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TotalPoints { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
