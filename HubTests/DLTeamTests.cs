using System;
using System.Collections.Generic;
using Xunit;
using Microsoft.EntityFrameworkCore;
using HubEntities.Database;
using HubDL;
using System.Linq;
using System.Threading.Tasks;

namespace HubTests {
    public class DLTeamTests {
        private readonly DbContextOptions<HubDbContext> _options;

        public DLTeamTests() {
            _options = new DbContextOptionsBuilder<HubDbContext>()
                .UseSqlite("Filename = dl_team_test.db").Options;
            Seed();
        }

        private void Seed() {
            using var context = new HubDbContext(_options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            User testOwner1 = new() {
                Id = "owner@gmail.com",
                Username = "owner@gmail.com"
            };

            User testOwner2 = new() {
                Id= "owner2@gmail.com",
                Username = "owner2@gmail.com"
            };

            context.Users.Add(testOwner1);
            context.Users.Add(testOwner2);

            context.Teams.AddRange(
                new() {
                    Id = "1",
                    Name = "Team1",
                    TeamOwner = testOwner1.Id,
                    Description = "We are Team1",
                    Users = new List<User> {
                        testOwner1
                    }
                },
                new() {
                    Id = "2",
                    Name = "Team2",
                    TeamOwner = testOwner2.Id,
                    Description = "We are Team2",
                    Users = new List<User> {
                        testOwner2
                    }
                },
                new() {
                    Id = "4",
                    Name = "Unique Name",
                    TeamOwner = testOwner1.Id,
                    Description = "This team is unique",
                    Users = new List<User> {
                        testOwner1
                    }
                }
            );

            context.SaveChanges();
        }

        [Fact]
        public async Task CreateTeam() {
            using var context = new HubDbContext(_options);
            IDatabase<Team> db = new HubDB<Team>(context);

            User newOwner = new() {
                Id = "owner3@gmail.com",
                Username = "owner3@gmail.com"
            };

            await db.Create(new() {
                Id = "3",
                Name = "Team3",
                TeamOwner = newOwner.Id,
                Description = "We are Team3"
            });

            Team targetTeam = context.Teams.Find("3");
            Assert.NotNull(targetTeam);
            Assert.IsType<Team>(targetTeam);
            Assert.Equal("Team3", targetTeam.Name);
            Assert.Equal("owner3@gmail.com", targetTeam.TeamOwner);
        }

        [Fact]
        public async Task DeleteTeam() {
            using var context = new HubDbContext(_options);
            IDatabase<Team> db = new HubDB<Team>(context);

            Team targetTeam = context.Teams.Find("2");
            await db.Delete(targetTeam);

            IList<Team> teams = context.Teams.Select(t => t).ToList();
            Assert.NotNull(teams);
            Assert.DoesNotContain(teams, t => t.Name == "Team2");
        }


        [Fact]
        public async Task QueryTeams() {
            using var context = new HubDbContext(_options);
            IDatabase<Team> db = new HubDB<Team>(context);

            IList<Team> results = await db.Query(new() {
                Conditions = new List<Func<Team, bool>>() {
                    p => p.Name.Contains("Team")
                }
            });

            Assert.NotNull(results);
            Assert.NotEmpty(results);
            Assert.Equal(2, results.Count);
            Assert.DoesNotContain(results, p => p.Name.Contains("Unique Name"));
        }

        [Fact]
        public async Task QuerySingleTeam() {
            using var context = new HubDbContext(_options);
            IDatabase<Team> db = new HubDB<Team>(context);

            Team result = await db.FindSingle(new() {
                Conditions = new List<Func<Team, bool>>() {
                    p => p.Name == "Unique Name"
                }
            });

            Team doesNotExist = await db.FindSingle(new() {
                Conditions = new List<Func<Team, bool>>() {
                    p => p.Name == "There is no team with this name"
                }
            });

            Assert.NotNull(result);
            Assert.Equal("Unique Name", result.Name);
            Assert.Null(doesNotExist);
        }

        [Fact]
        public async Task UpdateTeam() {
            using var context = new HubDbContext(_options);
            IDatabase<Team> db = new HubDB<Team>(context);

            await db.Update(new Team() {
                Id = "1",
                Name = "New Team Name"
            });

            Team result = context.Teams.Find("1");

            Assert.NotNull(result);
            Assert.Equal("New Team Name", result.Name);
        }
    }
}
