using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HubDL;
using HubEntities.Database;

namespace HubBL {
    public class ConnectionManager {
        private readonly IDatabase<ChatConnection> _connectionDB;
        private readonly IDatabase<User> _userDB;
        private readonly IList<string> _includes;

        public ConnectionManager(IDatabase<ChatConnection> connectionDB, IDatabase<User> userDB) {
            _connectionDB = connectionDB;
            _userDB = userDB;
            _includes = new List<string> {
                "User"
            };
        }

        public async Task<ChatConnection> GetConnection(string connectionId) {
            return await _connectionDB.FindSingle(new() {
                Includes = _includes,
                Conditions = new List<Func<ChatConnection, bool>> {
                    c => c.ConnectionId == connectionId
                }
            });
        }

        public async Task<bool> CloseConnection(string connectionId) {
            ChatConnection connection = await _connectionDB.FindSingle(new() {
                Includes = _includes,
                Conditions = new List<Func<ChatConnection, bool>> {
                    c => c.ConnectionId == connectionId
                }
            });
            if (connection == null) throw new ArgumentException("Unable to load connection");
            return await _connectionDB.Delete(connection);
        }

        public async Task<IList<User>> GetUsersByRoomId(string roomId) {
            IList<ChatConnection> connections = await _connectionDB.Query(new() {
                Includes = _includes,
                Conditions = new List<Func<ChatConnection, bool>> {
                    c => c.RoomId == roomId
                }
            });

            return connections.Select(c => c.User).ToList();
        }

        public async Task<bool> SaveConnection() {
            return await _userDB.Save();
        }
    }
}
