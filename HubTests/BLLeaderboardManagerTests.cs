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
    public class BLLeaderboardManagerTests {
        private readonly DbContextOptions<HubDbContext> _options;

        public BLLeaderboardManagerTests() {
            _options = new DbContextOptionsBuilder<HubDbContext>()
                .UseSqlite("Filename = bl_leaderboardmanager_test.db").Options;
            Seed();
        }

        private void Seed() {
            using var context = new HubDbContext(_options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Leaderboards.Add(new() {
                Id = "Game1",
                Scores = new HashSet<UserScore> {
                    new() { UserId = "1", Score = 1000},
                    new() { UserId = "2", Score = 2000},
                    new() { UserId = "3", Score = 3000},
                    new() { UserId = "1", Score = 4000},
                }
            });

            context.TeamLeaderboards.Add(new() {
                Id = "Game1",
                Scores = new HashSet<TeamScore> {
                    new() { TeamName = "team1", Score = 1000},
                    new() { TeamName = "team2", Score = 2000},
                    new() { TeamName = "team3", Score = 3000},
                }
            });

            context.SaveChanges();
        }

        [Fact]
        public async Task GetLeaderboardValid() {
            using var context = new HubDbContext(_options);
            LeaderboardManager leaderboardManager = new(new HubDB<Leaderboard>(context), new HubDB<TeamLeaderboard>(context));

            Leaderboard leaderboard = await leaderboardManager.GetLeaderboard("Game1");
            Leaderboard expectNull = await leaderboardManager.GetLeaderboard("Does not exist");
            Assert.Null(expectNull);

            Assert.NotNull(leaderboard);
            Assert.Equal("Game1", leaderboard.Id);
            Assert.NotNull(leaderboard.Scores);
            Assert.Contains(leaderboard.Scores, s => s.UserId == "1");
            Assert.Contains(leaderboard.Scores, s => s.UserId == "2");
            Assert.Contains(leaderboard.Scores, s => s.UserId == "3");
        }

        [Fact]
        public async Task GetLeaderboardInvalid() {
            using var context = new HubDbContext(_options);
            LeaderboardManager leaderboardManager = new(
                new HubDB<Leaderboard>(context), 
                new HubDB<TeamLeaderboard>(context)
            );

            await Assert.ThrowsAnyAsync<ArgumentException>(() =>
                leaderboardManager.GetLeaderboard(null)
            );
            
        }

        [Fact]
        public async Task SubmitScoreValid() {
            using var context = new HubDbContext(_options);
            LeaderboardManager leaderboardManager = new(
                new HubDB<Leaderboard>(context),
                new HubDB<TeamLeaderboard>(context)
            );

            Leaderboard result = await leaderboardManager.SubmitScore(
                "Game1", 
                new() { 
                    UserId = "5", 
                    Score = 1234 
                }
            );

            Assert.NotNull(result);
            Assert.Equal("Game1", result.Id);
            Assert.NotNull(result.Scores);
            Assert.Contains(result.Scores, s => s.UserId == "5" && s.Score == 1234);
        }

        [Theory]
        [InlineData(null, null, 0)]
        public async Task SubmitScoreInvalid(string gameName, string userId, int score) {
            using var context = new HubDbContext(_options);
            LeaderboardManager leaderboardManager = new(
                new HubDB<Leaderboard>(context),
                new HubDB<TeamLeaderboard>(context)
            );

            await Assert.ThrowsAnyAsync<ArgumentException>(() =>
                leaderboardManager.SubmitScore(gameName, new() {UserId= userId, Score = score })
            );
        }

        [Fact]
        public async Task GetTeamLeaderboardValid() {
            using var context = new HubDbContext(_options);
            LeaderboardManager leaderboardManager = new(new HubDB<Leaderboard>(context), new HubDB<TeamLeaderboard>(context));

            TeamLeaderboard leaderboard = await leaderboardManager.GetTeamLeaderboard("Game1");
            TeamLeaderboard expectNull = await leaderboardManager.GetTeamLeaderboard("Does not exist");
            Assert.Null(expectNull);

            Assert.NotNull(leaderboard);
            Assert.Equal("Game1", leaderboard.Id);
            Assert.NotNull(leaderboard.Scores);
            Assert.Contains(leaderboard.Scores, s => s.TeamName == "team1");
            Assert.Contains(leaderboard.Scores, s => s.TeamName == "team2");
            Assert.Contains(leaderboard.Scores, s => s.TeamName == "team3");
        }

        [Fact]
        public async Task GetTeamLeaderboardInvalid() {
            using var context = new HubDbContext(_options);
            LeaderboardManager leaderboardManager = new(
                new HubDB<Leaderboard>(context),
                new HubDB<TeamLeaderboard>(context)
            );

            await Assert.ThrowsAnyAsync<ArgumentException>(() =>
                leaderboardManager.GetTeamLeaderboard(null)
            );

        }

        [Fact]
        public async Task SubmitTeamScoreValid() {
            using var context = new HubDbContext(_options);
            LeaderboardManager leaderboardManager = new(
                new HubDB<Leaderboard>(context),
                new HubDB<TeamLeaderboard>(context)
            );

            TeamLeaderboard result = await leaderboardManager.SubmitTeamScore(
                "Game1",
                new() {
                    TeamName = "team5",
                    Score = 1234
                }
            );

            Assert.NotNull(result);
            Assert.Equal("Game1", result.Id);
            Assert.NotNull(result.Scores);
            Assert.Contains(result.Scores, s => s.TeamName == "team5" && s.Score == 1234);
        }

        [Theory]
        [InlineData(null, null, 0)]
        public async Task SubmitTeamScoreInvalid(string gameName, string userId, int score) {
            using var context = new HubDbContext(_options);
            LeaderboardManager leaderboardManager = new(
                new HubDB<Leaderboard>(context),
                new HubDB<TeamLeaderboard>(context)
            );

            await Assert.ThrowsAnyAsync<ArgumentException>(() =>
                leaderboardManager.SubmitTeamScore(gameName, new() { TeamName = userId, Score = score })
            );
        }
    }
}
