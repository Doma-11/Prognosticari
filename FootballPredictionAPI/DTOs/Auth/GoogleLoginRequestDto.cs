using System.ComponentModel.DataAnnotations;

namespace FootballPredictionAPI.DTOs.Auth
{
    public class GoogleLoginRequestDto
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}