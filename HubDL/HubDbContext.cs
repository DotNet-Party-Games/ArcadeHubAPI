using Microsoft.EntityFrameworkCore;
using HubEntities.Database;

namespace HubDL {
    public class HubDbContext: DbContext {
        public HubDbContext(): base() { }
        public HubDbContext(DbContextOptions options) : base() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<User>();
            modelBuilder.Entity<ChatMessage>();
        }
    }
}