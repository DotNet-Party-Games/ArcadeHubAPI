using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HubDL;
using HubEntities.Database;

namespace HubBL {
    public class MessageManager {
        private readonly IDatabase<ChatMessage> _messageDB;

        public MessageManager(IDatabase<ChatMessage> messageDB) {
            _messageDB = messageDB;
        }

        public async Task<ChatMessage> CreateMessage(ChatMessage message) {
            return await _messageDB.Create(message);
        }

        public async Task<IList<ChatMessage>> GetMessagesByUser(string userId) {
            return await _messageDB.Query(new() {
                Conditions = new List<Func<ChatMessage, bool>> {
                    m => m.SenderId == userId || m.ReceiverId == userId
                }
            }) ;
        }
    }
}
