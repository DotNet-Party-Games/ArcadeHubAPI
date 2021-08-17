using System;
using System.ComponentModel.DataAnnotations;

namespace HubEntities.Database {
    public class ChatMessage {
        [Key]
        public string Id { get; set; }
        public string SenderId { get; set; }
        public User Sender { get; set; }
        public string ReceiverId { get; set; }
        public User Receiver { get; set; }
        public string SenderName { get; set; }
        public string Body { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
