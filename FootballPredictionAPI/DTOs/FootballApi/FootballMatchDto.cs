namespace FootballPredictionAPI.DTOs.FootballApi
{
    public class FootballMatchDto
    {
        public int Id { get; set; }
        public DateTime UtcDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? Matchday { get; set; }
        public FootballCompetitionDto? Competition { get; set; }
        public FootballSeasonDto? Season { get; set; }

        public FootballTeamDto? HomeTeam { get; set; }
        public FootballTeamDto? AwayTeam { get; set; }

        public FootballScoreDto? Score { get; set; }
    }
}
