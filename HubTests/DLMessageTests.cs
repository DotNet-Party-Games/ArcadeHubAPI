using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using HubEntities.Database;
using HubDL;

namespace HubTests {
    public class DLMessageTests {
        private readonly DbContextOptions<HubDbContext> _options;

        public DLMessageTests() {
            _options = new DbContextOptionsBuilder<HubDbContext>()
                .UseSqlite("Filename = dl_team_test.db").Options;
            Seed();
        }

        private void Seed() {
            using var context = new HubDbContext(_options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Teams.AddRange();

            context.SaveChanges();
        }

        [Fact]
        public void CreateTeam() {

        }
    }
}
