using System.ComponentModel.DataAnnotations;

namespace FootballPredictionAPI.DTOs.Auth
{
    public class RegisterRequestDto
    {

        //Data Annotations 
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}