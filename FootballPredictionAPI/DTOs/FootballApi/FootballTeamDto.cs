namespace FootballPredictionAPI.DTOs.FootballApi
{
    public class FootballTeamDto
    {

        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? ShortName { get; set; }

        public string? Tla { get; set; }

        public string? Crest { get; set; }

        public string? ClubColors { get; set; }

        public string? Venue { get; set; }
    }
}
