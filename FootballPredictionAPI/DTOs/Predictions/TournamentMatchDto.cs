using System;

namespace FootballPredictionAPI.DTOs.Predictions
{
    public class TournamentMatchDto
    {
        public int Id { get; set; }

        public int HomeTeamId { get; set; }

        public int AwayTeamId { get; set; }

        public DateTime MatchDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public int? HomeTeamScore { get; set; }

        public int? AwayTeamScore { get; set; }
    }
}