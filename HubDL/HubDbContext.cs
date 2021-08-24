using Microsoft.EntityFrameworkCore;
using HubEntities.Database;

namespace HubDL {
    public class HubDbContext: DbContext {
        public HubDbContext(): base() { }
        public HubDbContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Leaderboard> Leaderboards { get; set; }
        public DbSet<TeamLeaderboard> TeamLeaderboards { get; set; }
        public DbSet<UserScore> UserScores { get; set; }
        public DbSet<TeamScore> TeamScores { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamJoinRequest> TeamJoinRequests { get; set; }
        public DbSet<ChatConnection> ChatConnections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<User>()
                .HasOne(u => u.Team)
                .WithMany(t => t.Users).OnDelete(DeleteBehavior.SetNull);

            //modelBuilder.Entity<ChatMessage>();
            modelBuilder.Entity<Leaderboard>();
            modelBuilder.Entity<TeamLeaderboard>();
            modelBuilder.Entity<UserScore>();
            modelBuilder.Entity<TeamScore>();
            modelBuilder.Entity<Team>().HasIndex(t => t.Name).IsUnique();
            modelBuilder.Entity<TeamJoinRequest>();
            modelBuilder.Entity<ChatConnection>();
        }
    }
}