using System.ComponentModel.DataAnnotations;

namespace FootballPredictionAPI.DTOs.Predictions
{
    public class UpdatePredictionDto
    {
        [Range(0, 30, ErrorMessage = "Predicted goals must be between 0 and 30.")]
        public int PredictedHomeGoals { get; set; }

        [Range(0, 30, ErrorMessage = "Predicted goals must be between 0 and 30.")]
        public int PredictedAwayGoals { get; set; }
    }

}