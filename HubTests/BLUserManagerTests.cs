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
    public class BLUserManagerTests {
        private readonly DbContextOptions<HubDbContext> _options;

        public BLUserManagerTests() {
            _options = new DbContextOptionsBuilder<HubDbContext>()
                .UseSqlite("Filename = bl_usermanager_test.db").Options;
            Seed();
        }

        private void Seed() {
            using var context = new HubDbContext(_options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Users.AddRange(
                new() {
                    Email = "user1@gmail.com",
                    Username = "user1",
                },
                new() {
                    Email = "user2@gmail.com",
                    Username = "user2",
                },
                new() {
                    Email = "uniqueuser@gmail.com",
                    Username = "uniqueuser",
                }
            );
            context.SaveChanges();
        }

        [Fact]
        public async Task CreateUserValid() {
            using var context = new HubDbContext(_options);
            UserManager userManager = new(new HubDB<User>(context));

            await userManager.CreateUser(new() {
                Email = "user3@gmail.com",
                Username = "user3"
            });

            User result = context.Users.Find("user3@gmail.com");

            Assert.NotNull(result);
            Assert.IsType<User>(result);
            Assert.Equal("user3", result.Username);
        }

        [Theory]
        [InlineData("user1@gmail.com", "user1")]
        [InlineData("user2@gmail.com", "user2")]
        [InlineData("uniqueuser@gmail.com", "uniqueuser")]
        [InlineData("user1@gmail.com", "newusername")]
        public async Task CreateUserInvalid(string email, string username) {
            using var context = new HubDbContext(_options);
            UserManager userManager = new(new HubDB<User>(context));

            await Assert.ThrowsAsync<DbUpdateException>(() =>
                userManager.CreateUser(new() {
                    Email = email,
                    Username = username
                })
            );
        }


        [Theory]
        [InlineData("user1@gmail.com", "user1")]
        [InlineData("user2@gmail.com", "user2")]
        [InlineData("uniqueuser@gmail.com", "uniqueuser")]
        [InlineData("doesnotexist@gmail.com", null)]
        public async Task GetUserValid(string userId, string expectedUserName) {
            using var context = new HubDbContext(_options);
            UserManager userManager = new(new HubDB<User>(context));

            User result = await userManager.GetUser(userId);

            if (expectedUserName != null) {
                Assert.NotNull(result);
                Assert.IsType<User>(result);
                Assert.Equal(expectedUserName, result.Username);
            } else {
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task GetUserInvalid() {
            using var context = new HubDbContext(_options);
            UserManager userManager = new(new HubDB<User>(context));
            await Assert.ThrowsAsync<ArgumentException>(() =>
                userManager.GetUser(null)
            );
        }

        [Theory]
        [InlineData("user1@gmail.com", "user1")]
        [InlineData("user2@gmail.com", "user2")]
        public async Task EditProfileValid(string userId, string username) {
            using var context = new HubDbContext(_options);
            UserManager userManager = new(new HubDB<User>(context));

            await userManager.EditProfile(new() {
                Email = userId,
                Username = username
            });

            User targetUser = context.Users.Find(userId);
            Assert.NotNull(targetUser);
            Assert.IsType<User>(targetUser);
            Assert.Equal(username, targetUser.Username);
        }


        [Theory]
        [InlineData("user1@gmail.com", "user2")]
        [InlineData("user1@gmail.com", "uniqueuser")]
        [InlineData("user2@gmail.com", "user1")]
        public async Task EditProfileInvalid(string userId, string username) {
            using var context = new HubDbContext(_options);
            UserManager userManager = new(new HubDB<User>(context));

            await Assert.ThrowsAsync<DbUpdateException>(() =>
                userManager.EditProfile(new() {
                    Email = userId,
                    Username = username
                })
            );
        }
    }
}
