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
            await Clients.Group(gameName).SendAsync("Event", $"{userId} joined");
        }

        public async Task LeaveGameLobbyChat(string gameName, string userId) {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameName);
            await Clients.Group(gameName).SendAsync("Event", $"{userId} left");
        }

        // Game Room Chat
        public async Task GameRoomChat(string roomId, ChatMessage message) {
            await Clients.Group(roomId).SendAsync("Message", message);

        }
        public async Task JoinGameRoomChat(string roomId, string userId) {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("Event", $"{userId} joined");
        }

        public async Task LeaveGameRoomChat(string roomId, string userId) {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("Event", $"{userId} left");
        }

        // Team Chat
        public async Task TeamChat(string teamId, ChatMessage message) {
            await Clients.Group(teamId).SendAsync("Message", message);

        }
        public async Task JoinTeamChat(string teamId, string userId) {
            await Groups.AddToGroupAsync(Context.ConnectionId, teamId);
            await Clients.Group(teamId).SendAsync("Event", $"{userId} joined");
        }

        public async Task LeaveTeamChat(string teamId, string userId) {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, teamId);
            await Clients.Group(teamId).SendAsync("Event", $"{userId} left");
        }
    }
}