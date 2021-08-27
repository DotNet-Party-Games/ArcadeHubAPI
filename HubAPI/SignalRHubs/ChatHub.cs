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
using HubBL;
using AutoMapper;

namespace HubAPI {

    [Authorize]
    public class ChatHub: Hub {
        private readonly UserManager _userManager;
        private readonly ConnectionManager _connectionManager;
        private readonly IMapper _mapper;
        public ChatHub(UserManager userManager, ConnectionManager connectionManager, IMapper mapper) {
            _userManager = userManager;
            _connectionManager = connectionManager;
            _mapper = mapper;
        }

        public override Task OnConnectedAsync() {
            string userId = Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            User user = _userManager.GetUser(userId).Result;
            if (user == null) throw new ArgumentException($"Unable to load user with Id {userId}");

            user.Connections.Add(new() {
                ConnectionId = Context.ConnectionId,
                Connected = true
            });

            if (!_userManager.SaveUser().Result) {
                throw new ArgumentException($"Unable to save user with Id {userId}");
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception) {
            ChatConnection connection = _connectionManager.GetConnection(Context.ConnectionId).Result;
            if (connection != null) {
                if (connection.RoomId != null) {
                    Clients.Group(connection.RoomId).SendAsync("Event", new ChatStatusDto() {
                        User = _mapper.Map<UserDto>(connection.User),
                        Status = "LEFT"
                    });
                }
                if (!_connectionManager.CloseConnection(Context.ConnectionId).Result) {
                    throw new ArgumentException("Unable to close the connection in the datbase.");
                }
            }
            return base.OnDisconnectedAsync(exception);
        }

        // Game Room Chat
        public async Task GameRoomChat(string roomId, ChatMessage message) {
            await Clients.Group(roomId).SendAsync("Message", message);
        }
        public async Task JoinGameRoomChat(string roomId) {
            string userId = Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            ChatConnection userConnection = await _connectionManager.GetConnection(Context.ConnectionId);
            if (userConnection == null) {
                throw new ArgumentException($"Unable to find connection with ID {Context.ConnectionId} in database");
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            userConnection.RoomId = roomId;
            await Clients.Group(userConnection.RoomId).SendAsync("Event", new ChatStatusDto() {
                User = _mapper.Map<UserDto>(userConnection.User),
                Status = "JOINED"
            });

            IList<User> usersInRoom = await _connectionManager.GetUsersByRoomId(roomId);
            if (usersInRoom != null) {
                foreach (User user in usersInRoom) {
                    await Clients.Client(Context.ConnectionId).SendAsync("Event", new ChatStatusDto() {
                        User = _mapper.Map<UserDto>(user),
                        Status = "PRESENT"
                    });
                }
            }

            if (!_userManager.SaveUser().Result) {
                throw new ArgumentException($"Unable to save connection status for user with ID {userId} joining room {roomId}");
            }
        }

        public async Task LeaveGameRoomChat(string roomId) {
            string userId = Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            ChatConnection userConnection = await _connectionManager.GetConnection(Context.ConnectionId);
            await Clients.Group(userConnection.RoomId).SendAsync("Event", new ChatStatusDto() {
                User = _mapper.Map<UserDto>(userConnection.User),
                Status = "LEFT"
            });
            userConnection.RoomId = null;
            if (!_userManager.SaveUser().Result) {
                throw new ArgumentException($"Unable to save connection status for user with ID {userId} joining room {roomId}");
            }
        }
    }
}