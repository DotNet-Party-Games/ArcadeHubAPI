using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HubDL;
using HubEntities.Database;

namespace HubBL {
    public class UserManager {
        private readonly IDatabase<User> _userDB;

        public UserManager(IDatabase<User> userDB) {
            _userDB = userDB;
        }

        public async Task<User> CreateUser(User user) {
            return await _userDB.Create(user);
        }

        public async Task<User> GetUser(string userId) {
            return await _userDB.FindSingle(new() {
                Conditions = new List<Func<User, bool>> {
                    u => u.Email == userId
                }
            });
        }

        public async Task<bool> EditProfile(User user) {
            return await _userDB.Update(user);
        }
    }
}