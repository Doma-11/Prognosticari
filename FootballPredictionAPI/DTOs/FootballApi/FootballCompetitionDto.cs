namespace FootballPredictionAPI.DTOs.FootballApi
{
    public class FootballCompetitionDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string? Emblem { get; set; }

        public string? Plan { get; set; }

        public FootballAreaDto? Area { get; set; }

        public FootballSeasonDto? CurrentSeason { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
