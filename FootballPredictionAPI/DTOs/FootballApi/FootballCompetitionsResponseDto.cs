namespace FootballPredictionAPI.DTOs.FootballApi
{
    public class FootballCompetitionsResponseDto
    {
        public int Count { get; set; }

        public FootballCompetitionFiltersDto? Filters { get; set; }

        public List<FootballCompetitionDto> Competitions { get; set; } = new ();
    }

    public class FootballCompetitionFiltersDto
    {
        public string? Client { get; set; }
    }
}
