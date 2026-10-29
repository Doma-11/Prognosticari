namespace FootballPredictionAPI.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public UserResponseDto User { get; set; } = new();
    }
}