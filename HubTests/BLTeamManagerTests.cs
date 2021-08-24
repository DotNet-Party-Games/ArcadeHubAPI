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
                .UseSqlite("Filename = bl_teammanager_test.db").Options;
            Seed();
        }

        private void Seed() {
            using var context = new HubDbContext(_options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            User user1 = new() {
                Id = "user1@gmail.com",
                Username = "user1",
            };

            User user2 = new() {
                Id = "user2@gmail.com",
                Username = "user2",
            };

            User user3 = new() {
                Id = "uniqueuser@gmail.com",
                Username = "uniqueuser",
            };

            User user4 = new() {
                Id = "user4@gmail.com",
                Username = "user4",
            };

            User user5 = new() {
                Id = "user5@gmail.com",
                Username = "user5",
            };

            context.Users.AddRange(user1, user2, user3);

            context.Teams.AddRange(
                new() {
                    Id = "1",
                    Name = "Team1",
                    TeamOwner = user1.Id,
                    Description = "We are Team1",
                    Users = new List<User> {
                        user1
                    }
                },
                new() {
                    Id = "2",
                    Name = "Team2",
                    TeamOwner = user2.Id,
                    Description = "We are Team2",
                    Users = new List<User> {
                        user2, user4
                    }
                },
                new() {
                    Id = "3",
                    Name = "Unique Name",
                    TeamOwner = user3.Id,
                    Description = "This team is unique",
                    Users = new List<User> {
                        user5
                    }
                }
            );

            context.TeamJoinRequests.Add(new() {
                Id = "testId",
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

            Team newTeam = await teamManager.CreateTeam("New Team", "This is a new team", "uniqueuser@gmail.com");

            Assert.NotNull(newTeam);
            Assert.NotNull(newTeam.Id);
            Team targetTeam = context.Teams.Include(t => t.Users).SingleOrDefault(t => t.Id == newTeam.Id);
            Assert.NotNull(targetTeam);
            Assert.IsType<Team>(targetTeam);
            Assert.Equal("New Team", targetTeam.Name);
            Assert.NotNull(targetTeam.Users);
            Assert.Contains(targetTeam.Users, u => u.Id == "uniqueuser@gmail.com");
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
                teamManager.CreateTeam(teamname, "This is a new team", "uniqueuser@gmail.com")
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

            TeamJoinRequest newRequest = await teamManager.CreateRequest("uniqueuser@gmail.com","Team1");
            Assert.NotNull(newRequest);

            TeamJoinRequest targetRequest = context.TeamJoinRequests.Find(newRequest.Id);
            Assert.NotNull(targetRequest);
            Assert.Equal("Team1", targetRequest.TeamName);
            Assert.Equal("uniqueuser@gmail.com", targetRequest.UserId);
        }

        [Theory]
        [InlineData("Team1", "doesnotexist")]
        [InlineData("doesnotexist", "uniqueuser@gmail.com")]
        public async Task CreateRequestInvalid(string teamname, string userId) {
            using var context = new HubDbContext(_options);
            TeamManager teamManager = new(
                new HubDB<Team>(context),
                new HubDB<TeamJoinRequest>(context),
                new HubDB<User>(context)
            );

            await Assert.ThrowsAsync<ArgumentException>(() =>
                teamManager.CreateRequest(teamname, userId)
            );
        }

        [Theory]
        [InlineData("Team2", "uniqueuser@gmail.com")]
        public async Task CreateRequestDuplicate(string teamname, string userId) {
            using var context = new HubDbContext(_options);
            TeamManager teamManager = new(
                new HubDB<Team>(context),
                new HubDB<TeamJoinRequest>(context),
                new HubDB<User>(context)
            );

            await Assert.ThrowsAsync<ArgumentException>(() =>
                teamManager.CreateRequest(teamname, userId)
            );
        }

        [Theory]
        [InlineData("Team2", 1)]
        [InlineData("Team1", 0)]
        public async Task GetRequestsValid(string teamName, int count) {
            using var context = new HubDbContext(_options);
            TeamManager teamManager = new(
                new HubDB<Team>(context),
                new HubDB<TeamJoinRequest>(context),
                new HubDB<User>(context)
            );

            IList<TeamJoinRequest> results = await teamManager.GetRequestsByTeamName(teamName);

            Assert.NotNull(results);
            Assert.Equal(count, results.Count);
        }

        [Theory]
        [InlineData(null)]
        public async Task CreateRequestsInvalid(string teamName) {
            using var context = new HubDbContext(_options);
            TeamManager teamManager = new(
                new HubDB<Team>(context),
                new HubDB<TeamJoinRequest>(context),
                new HubDB<User>(context)
            );

            await Assert.ThrowsAsync<ArgumentException>(() =>
                teamManager.GetRequestsByTeamName(teamName)
            );
        }

        [Fact]
        public async Task ApproveRequest() {
            using var context = new HubDbContext(_options);
            TeamManager teamManager = new(
                new HubDB<Team>(context),
                new HubDB<TeamJoinRequest>(context),
                new HubDB<User>(context)
            );

            await teamManager.ApproveOrDenyRequest("testId", "user2@gmail.com");

            Team targetTeam = context.Teams.Include(t => t.Users).SingleOrDefault(t => t.Name == "Team2");

            TeamJoinRequest targetRequest = context.TeamJoinRequests.Find("testId");
            Assert.Null(targetRequest);
            Assert.NotNull(targetTeam);
            Assert.NotNull(targetTeam.Users);
            Assert.Contains(targetTeam.Users, t => t.Id == "uniqueuser@gmail.com");
        }

        [Fact]
        public async Task DenyRequest() {
            using var context = new HubDbContext(_options);
            TeamManager teamManager = new(
                new HubDB<Team>(context),
                new HubDB<TeamJoinRequest>(context),
                new HubDB<User>(context)
            );

            await teamManager.ApproveOrDenyRequest("testId", "user2@gmail.com", false);

            Team targetTeam = context.Teams.Include(t => t.Users).SingleOrDefault(t => t.Name == "Team2");

            TeamJoinRequest targetRequest = context.TeamJoinRequests.Find("testId");
            Assert.Null(targetRequest);
            Assert.NotNull(targetTeam);
            Assert.NotNull(targetTeam.Users);
            Assert.DoesNotContain(targetTeam.Users, t => t.Id == "uniqueuser@gmail.com");
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("testId", "user1@gmail.com")]
        [InlineData(null, "user2@gmail.com")]
        public async Task ApproveRequestBadArgs(string requestId, string ownerId) {
            using var context = new HubDbContext(_options);
            TeamManager teamManager = new(
                new HubDB<Team>(context),
                new HubDB<TeamJoinRequest>(context),
                new HubDB<User>(context)
            );

            await Assert.ThrowsAsync<ArgumentException>(() =>
                teamManager.ApproveOrDenyRequest(requestId, ownerId)
            );
        }

        [Fact]
        public async Task LeaveTeamValid() {
            using var context = new HubDbContext(_options);
            TeamManager teamManager = new(
                new HubDB<Team>(context),
                new HubDB<TeamJoinRequest>(context),
                new HubDB<User>(context)
            );

            await teamManager.LeaveTeam("user2@gmail.com");

            Team targetTeam = context.Teams.Include(t => t.Users).SingleOrDefault(t => t.Name == "Team2");

            Assert.NotNull(targetTeam);
            Assert.NotNull(targetTeam.Users);
            Assert.DoesNotContain(targetTeam.Users, t => t.Id == "user2@gmail.com");
        }

        [Theory]
        [InlineData("does not exist")]
        public async Task LeaveTeamInvalid(string userId) {
            using var context = new HubDbContext(_options);
            TeamManager teamManager = new(
                new HubDB<Team>(context),
                new HubDB<TeamJoinRequest>(context),
                new HubDB<User>(context)
            );

            await Assert.ThrowsAsync<ArgumentException>(() =>
                teamManager.LeaveTeam(userId)
            );
        }

        [Theory]
        [InlineData("user1@gmail.com", "Team1")]
        [InlineData("user2@gmail.com", "Team2")]
        [InlineData("uniqueuser@gmail.com", "Unique Name")]
        public async Task DisbandTeamValid(string userId, string teamName) {
            using var context = new HubDbContext(_options);
            TeamManager teamManager = new(
                new HubDB<Team>(context),
                new HubDB<TeamJoinRequest>(context),
                new HubDB<User>(context)
            );

            await teamManager.DisbandTeam(userId, teamName);

            Team targetTeam = context.Teams.Include(t => t.Users).SingleOrDefault(t => t.Name == teamName);

            Assert.Null(targetTeam);
        }

        [Theory]
        [InlineData("user1@gmail.com", null)]
        [InlineData(null, "Team1")]
        [InlineData("user2@gmail.com", "Team1")]
        public async Task DisbandTeamInvalid(string userId, string teamName) {
            using var context = new HubDbContext(_options);
            TeamManager teamManager = new(
                new HubDB<Team>(context),
                new HubDB<TeamJoinRequest>(context),
                new HubDB<User>(context)
            );

            await Assert.ThrowsAsync<ArgumentException>(() =>
                teamManager.DisbandTeam(userId, teamName)
            );
        }
    }
}
