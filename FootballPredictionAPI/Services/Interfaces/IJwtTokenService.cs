using FootballPredictionAPI.Models;


namespace FootballPredictionAPI.Services.Interfaces
{
    public interface IJwtTokenService
    {
        //the method returns a JWT as text.
        string GenerateToken(User user);
    }
}
