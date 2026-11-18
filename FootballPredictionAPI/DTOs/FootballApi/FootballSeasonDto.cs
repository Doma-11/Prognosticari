namespace FootballPredictionAPI.DTOs.FootballApi
{
    public class FootballSeasonDto
    {
        public int Id { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public int? CurrentMatchday { get; set; }

        public FootballTeamDto? Winner { get; set; }
    }
}
