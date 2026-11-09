namespace FootballPredictionAPI.DTOs.Tournaments
{
    public class CreatedTournamentResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string InvitationCode { get; set; } = string.Empty;
    }
}
