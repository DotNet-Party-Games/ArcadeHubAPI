using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using HubEntities.Database;
using HubDL;
using HubBL;

namespace HubTests {
    public class BLTeamManagerTests {
        private readonly DbContextOptions<HubDbContext> _options;

        public BLTeamManagerTests() {
            _options = new DbContextOptionsBuilder<HubDbContext>()
                .UseSqlite("Filename = bl_usermanager_test.db").Options;
            Seed();
        }

        private void Seed() {
            using var context = new HubDbContext(_options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            User user1 = new() {
                Email = "user1@gmail.com",
                Username = "user1",
            };

            User user2 = new() {
                Email = "user2@gmail.com",
                Username = "user2",
            };

            User user3 = new() {
                Email = "uniqueuser@gmail.com",
                Username = "uniqueuser",
            };

            context.Users.AddRange(user1, user2, user3);

            context.Teams.AddRange(
                new() {
                    Id = "1",
                    Name = "Team1",
                    TeamOwner = user1.Email,
                    Description = "We are Team1",
                    Users = new List<User> {
                        user1
                    }
                },
                new() {
                    Id = "2",
                    Name = "Team2",
                    TeamOwner = user2.Email,
                    Description = "We are Team2",
                    Users = new List<User> {
                        user2, user1
                    }
                },
                new() {
                    Id = "3",
                    Name = "Unique Name",
                    TeamOwner = user1.Email,
                    Description = "This team is unique",
                    Users = new List<User> {
                        user1, user3
                    }
                }
            );

            context.TeamJoinRequests.Add(new() { 
                TeamName = "Team2",
                UserId = "uniqueuser@gmail.com"
            });
            context.SaveChanges();
        }

        [Fact]
        public async Task CreateValid() {
            using var context = new HubDbContext(_options);
            TeamManager teamManager = new(
                new HubDB<Team>(context), 
                new HubDB<TeamJoinRequest>(context), 
                new HubDB<User>(context)
            );

            Team newTeam = await teamManager.CreateTeam("New Team", "This is a new team", "user1@gmail.com");

            Assert.NotNull(newTeam);
            Assert.NotNull(newTeam.Id);
            Team targetTeam = context.Teams.Include(t => t.Users).SingleOrDefault(t => t.Id == newTeam.Id);
            Assert.NotNull(targetTeam);
            Assert.IsType<Team>(targetTeam);
            Assert.Equal("New Team", targetTeam.Name);
            Assert.NotNull(targetTeam.Users);
            Assert.Contains(targetTeam.Users, u => u.Email == "user1@gmail.com");
        }

        [Theory]
        [InlineData("Team1")]
        [InlineData("Team2")]
        public async Task CreateInvalidTeamName(string teamname) {
            using var context = new HubDbContext(_options);
            TeamManager teamManager = new(
                new HubDB<Team>(context),
                new HubDB<TeamJoinRequest>(context),
                new HubDB<User>(context)
            );

            await Assert.ThrowsAsync<DbUpdateException>(() => 
                teamManager.CreateTeam(teamname, "This is a new team", "user1@gmail.com")
            );
        }

        [Theory]
        [InlineData("Team1", "Hi", "doesnotexist")]
        [InlineData("Team2", null, "user1@gmail.com")]
        [InlineData(null, "Hi", "doesnotexist")]
        [InlineData("Team2", "Hi", null)]
        public async Task CreateTeamBadArgs(string teamname, string description, string ownerId) {
            using var context = new HubDbContext(_options);
            TeamManager teamManager = new(
                new HubDB<Team>(context),
                new HubDB<TeamJoinRequest>(context),
                new HubDB<User>(context)
            );

            await Assert.ThrowsAsync<ArgumentException>(() =>
                teamManager.CreateTeam(teamname, description, ownerId)
            );
        }

        [Fact]
        public async Task CreateRequestValid() {
            using var context = new HubDbContext(_options);
            TeamManager teamManager = new(
                new HubDB<Team>(context),
                new HubDB<TeamJoinRequest>(context),
                new HubDB<User>(context)
            );

            TeamJoinRequest newRequest = await teamManager.CreateRequest("Team1", "uniqueuser@gmail.com");
            Assert.NotNull(newRequest);

            TeamJoinRequest targetRequest = context.TeamJoinRequests.Find(newRequest.Id);

        }
    }
}
