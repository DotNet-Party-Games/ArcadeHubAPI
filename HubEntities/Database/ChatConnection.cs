using System.ComponentModel.DataAnnotations;

namespace HubEntities.Database {
    public class ChatConnection {
        [Key]
        public string ConnectionId { get; set; }
        public string RoomId { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public bool Connected { get; set; }
    }
}