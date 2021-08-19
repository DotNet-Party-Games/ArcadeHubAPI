using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

using HubEntities.Database;

namespace HubAPI {
    public class ChatHub: Hub {

        // Game Lobby Chat
        public async Task GameLobbyChat(string gameName, ChatMessage message) {
            await Clients.Group(gameName).SendAsync("Message", message);
        }

        public async Task JoinGameLobbyChat(string gameName, string userId) {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameName);
            await Clients.Group(gameName).SendAsync()
        }

        public async Task LeaveGameLobbyChat(string gameName) {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameName);
        }

        // Game Room Chat
        public async Task GameRoomChat(string roomId, ChatMessage message) {
            await Clients.Group(roomId).SendAsync("Message", message);

        }
    }
}