using FootballPredictionAPI.DTOs.Tournaments;

namespace FootballPredictionAPI.Services.Interfaces
{
    public interface ITournamentService
    {
        //Creates a tournament owned by the authenticated user.
        Task<CreatedTournamentResponseDto> CreateTournamentAsync(CreateTournamentRequestDto request, int currentUserId);

        //Retrieves one tournament, or null if not found or inaccessible.
        Task<TournamentResponseDto?> GetTournamentByIdAsync(int tournamentId, int currentUserId);

        //Lists tournaments the current user participates in.
        Task<List<TournamentResponseDto>> GetMyTournamentsAsync(int currentUserId);

        //Joins a tournament using its invitation code.
        Task<JoinTournamentResponseDto> JoinTournamentAsync(JoinTournamentRequestDto request, int currentUserId);

        //Retrieves the participants of a tournament.
        Task<List<TournamentParticipantResponseDto>> GetParticipantsAsync(int tournamentId, int currentUserId);

        //	Updates tournament details after checking ownership.
        Task<TournamentResponseDto> UpdateTournamentAsync(int tournamentId, UpdateTournamentRequestDto request, int currentUserId);

        //Removes the current user from a tournament, subject to membership rules.
        Task LeaveTournamentAsync(int tournamentId, int currentUserId);


    }
}
