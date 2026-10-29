using Microsoft.EntityFrameworkCore;
using FootballPredictionAPI.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace FootballPredictionAPI.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            //RELATIONSHIP CONFIGURATIONS

            //This tells EF Core that each match belongs to one season, each season has many matches, and SeasonId is the foreign key.
            modelBuilder.Entity<Match>()
                .HasOne(m => m.Season)
                .WithMany(s => s.Matches)
                .HasForeignKey(m => m.SeasonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Season>()
                .HasOne(s => s.League)
                .WithMany(l => l.Seasons)
                .HasForeignKey(s => s.LeagueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TournamentParticipant>()
                .HasOne(t => t.User)
                .WithMany(u => u.TournamentParticipations)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TournamentParticipant>()
                .HasOne(tp => tp.Tournament)
                .WithMany(t => t.Participants)
                .HasForeignKey(tp => tp.TournamentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prediction>()
                .HasOne(p=>p.TournamentParticipant)
                .WithMany(tp=> tp.Predictions)
                .HasForeignKey(p=>p.TournamentParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Each prediction belongs to one match.
            modelBuilder.Entity<Prediction>()
                .HasOne(p => p.Match)
                .WithMany(m => m.Predictions)
                .HasForeignKey(p => p.MatchId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Team -> Home Matches
            // One team can play at home in many matches.
            modelBuilder.Entity<Match>()
                .HasOne(m => m.HomeTeam)
                .WithMany(t => t.HomeMatches)
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Team -> Away Matches
            // One team can play away in many matches.
            modelBuilder.Entity<Match>()
                .HasOne(m=>m.AwayTeam)
                .WithMany(t=>t.AwayMatches)
                .HasForeignKey(m=>m.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Season -> Tournaments
            // One season can have many tournaments.
            modelBuilder.Entity<Tournament>()
                .HasOne(t => t.Season)
                .WithMany(s => s.Tournaments)
                .HasForeignKey(t => t.SeasonId)
                .OnDelete(DeleteBehavior.Restrict);



            // 5. User -> Created Tournaments
            // One user can create many tournaments.
            modelBuilder.Entity<Tournament>()
                .HasOne(t => t.CreatedByUser)
                .WithMany(u => u.CreatedTournaments)
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);


            //INDEX CONFIGURATIONS


            //combination(TournamentId = 10, UserId = 5) can exist only once.
            modelBuilder.Entity<TournamentParticipant>()
                .HasIndex(tp=> new {tp.TournamentId, tp.UserId})
                .IsUnique();

            //one participant can predict a particular match only once within their tournament.
            modelBuilder.Entity<Prediction>()
                .HasIndex(p => new { p.TournamentParticipantId, p.MatchId })
            .IsUnique();


            //Every tournament needs its own invitation code.
            modelBuilder.Entity<Tournament>()
                .HasIndex(t => t.InvitationCode)
                .IsUnique();

            //prevent duplicate leagues from football API
            modelBuilder.Entity<League>()
                .HasIndex(l => l.ExternalApiId)
                .IsUnique();

            modelBuilder.Entity<Team>()
                .HasIndex(t=>t.ExternalApiId)
                .IsUnique();

            modelBuilder.Entity<Match>()
                .HasIndex(m=>m.ExternalApiId)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            //a league cannot have two seasons with the same name.
            modelBuilder.Entity<Season>()
                .HasIndex(s => new { s.LeagueId, s.Name })
                .IsUnique();


            //google
            modelBuilder.Entity<User>()
                .HasIndex(u => u.GoogleId)
                .IsUnique();
        }
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Tournament> Tournaments { get; set; } = null!;
        public DbSet<TournamentParticipant> TournamentParticipants { get; set; } = null!;
        public DbSet<League> Leagues { get; set; } = null!;
        public DbSet<Team> Teams { get; set; } = null!;
        public DbSet<Season> Seasons { get; set; } = null!;
        public DbSet<Match> Matches { get; set; } = null!;
        public DbSet<Prediction> Predictions { get; set; } = null!;
    }
}
