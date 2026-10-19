using FootballPredictionAPI.Enums;

namespace FootballPredictionAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string? GoogleId { get; set; }
        public UserRole Role { get; set; } = UserRole.Player;
        public DateTime CreatedAt { get; set; }

        public ICollection<TournamentParticipant> TournamentParticipations { get; set; } = new List<TournamentParticipant>();

        //One user can create multiple tournaments.
        public ICollection<Tournament> CreatedTournaments { get; set; } = new List<Tournament>();

    }
}
