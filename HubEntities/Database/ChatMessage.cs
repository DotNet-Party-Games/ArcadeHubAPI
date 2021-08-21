using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HubEntities.Database {
    public class ChatMessage {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
