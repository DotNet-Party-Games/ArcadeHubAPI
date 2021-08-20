using Microsoft.EntityFrameworkCore;
using HubEntities.Database;

namespace HubDL {
    public class HubDbContext: DbContext {
        public HubDbContext(): base() { }
        public HubDbContext(DbContextOptions options) : base() { }

        public DbSet<User> Users { get; set; }
        public DbSet<Leaderboard> Leaderboards { get; set; }
        public DbSet<TeamLeaderboard> TeamLeaderBoards { get; set; }
        public DbSet<UserScore> UserScores { get; set; }
        public DbSet<TeamScore> TeamScores { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamJoinRequest> TeamJoinRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<User>();
            //modelBuilder.Entity<ChatMessage>();
            modelBuilder.Entity<Leaderboard>();
            modelBuilder.Entity<TeamLeaderboard>();
            modelBuilder.Entity<UserScore>();
            modelBuilder.Entity<TeamScore>();
            modelBuilder.Entity<Team>();
            modelBuilder.Entity<TeamJoinRequest>();
        }
    }
}