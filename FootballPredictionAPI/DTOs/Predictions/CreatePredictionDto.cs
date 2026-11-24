using System.ComponentModel.DataAnnotations;

namespace FootballPredictionAPI.DTOs.Predictions
{
    public class CreatePredictionDto
    {
        [Range(1, int.MaxValue)]
        public int MatchId { get; set; }

        [Range(0, 30, ErrorMessage = "Predicted goals must be between 0 and 30.")]
        public int PredictedHomeGoals { get; set; }

        [Range(0, 30, ErrorMessage = "Predicted goals must be between 0 and 30.")]
        public int PredictedAwayGoals { get; set; }
    }
}

