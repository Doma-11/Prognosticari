namespace FootballPredictionAPI.DTOs.FootballApi
{
    public class FootballTeamsResponseDto
    {
        public int Count { get; set; }
        public List<FootballTeamDto> Teams { get; set; } = new();
    }
}
