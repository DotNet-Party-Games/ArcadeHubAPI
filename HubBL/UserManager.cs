using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HubDL;
using HubEntities.Database;

namespace HubBL {
    public class UserManager {
        private readonly IDatabase<User> _userDB;
        private readonly IList<string> _includes;

        public UserManager(IDatabase<User> userDB) {
            _userDB = userDB;
            _includes = new List<string> {
                "Team"
            };
        }

        public async Task<User> CreateUser(User user) {
            return await _userDB.Create(user);
        }

        public async Task<User> GetUser(string userId) {
            if (userId == null) throw new ArgumentException("Missing parameter userId");
            return await _userDB.FindSingle(new() {
                Conditions = new List<Func<User, bool>> {
                    u => u.Id == userId
                },
                Includes = _includes
            });
        }

        public async Task<User> EditProfile(User user) {
            if (user == null) throw new ArgumentException("Missing parameter user");

            User targetUser = await _userDB.FindSingle(new() {
                Conditions = new List<Func<User, bool>> {
                    u => u.Id == user.Id
                },
                Includes = _includes
            });

            if (targetUser == null) {
                throw new ArgumentException($"Unable to find user with given id \"{user.Id}\"");
            }

            targetUser.Username = user.Username;
            await _userDB.Save();
            return targetUser;
        }
    }
}