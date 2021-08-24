using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

using HubEntities.Database;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;
using System.Linq;
using HubDL;
using HubEntities.Dto;
using Microsoft.EntityFrameworkCore;

namespace HubAPI {

    [Authorize]
    public class ChatHub: Hub {
        public override Task OnConnectedAsync() {
            string userId = Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            using (HubDbContext db = new()) {
                User user = db.Users.Include(u => u.Connections).SingleOrDefault(u => u.Id == userId);
                if (user == null) {
                    throw new ArgumentNullException("Unable to load user");
                }

                user.Connections.Add(new() {
                    ConnectionId = Context.ConnectionId,
                    Connected = true
                });
                db.SaveChanges();
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception) {
            using (HubDbContext db = new()) {
                ChatConnection connection = db.ChatConnections
                    .Include(c => c.User)
                    .Where(c => c.ConnectionId == Context.ConnectionId)
                    .SingleOrDefault();

                connection.Connected = false;
                if (connection.RoomId != null) {
                    Clients.Group(connection.RoomId).SendAsync("Event", new ChatStatusDto() {
                        User = connection.User,
                        Status = "LEFT"
                    });
                    connection.RoomId = null;
                }
                db.Remove(connection);
                db.SaveChanges();
            }
            return base.OnDisconnectedAsync(exception);
        }

        // Game Room Chat
        public async Task GameRoomChat(string roomId, ChatMessage message) {
            await Clients.Group(roomId).SendAsync("Message", message);

        }
        public async Task JoinGameRoomChat(string roomId) {
            string userId = Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            using (HubDbContext db = new()) {
                ChatConnection connection = db.ChatConnections
                    .Include(c => c.User)
                    .Where(c => c.ConnectionId == Context.ConnectionId)
                    .SingleOrDefault();
                await Clients.Group(connection.RoomId).SendAsync("Event", new ChatStatusDto() {
                    User = connection.User,
                    Status = "JOINED"
                });
                connection.RoomId = roomId;
                db.SaveChanges();

                //Get all users with roomId
                IList<User> usersInRoom = db.ChatConnections
                    .Include(c => c.User)
                    .Where(c => c.RoomId == roomId)
                    .Select(c => c.User)
                    .ToList();

                foreach(User user in usersInRoom) {
                    await Clients.Group(roomId).SendAsync("Event", new ChatStatusDto() {
                        User = user,
                        Status = "PRESENT"
                    });
                }

            }

        }

        public async Task LeaveGameRoomChat(string roomId) {
            string userId = Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            using (HubDbContext db = new()) {
                ChatConnection connection = db.ChatConnections
                    .Include(c => c.User)
                    .Where(c => c.ConnectionId == Context.ConnectionId)
                    .SingleOrDefault();

                if (connection.RoomId != null) {
                    await Clients.Group(connection.RoomId).SendAsync("Event", new ChatStatusDto() {
                        User = connection.User,
                        Status = "LEFT"
                    });
                    connection.RoomId = null;
                }

                db.SaveChanges();
            }
        }
    }
}