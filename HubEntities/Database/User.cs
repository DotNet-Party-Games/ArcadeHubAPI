using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HubEntities.Database {
    public class User {
        [Key]
        public string Id { get; set; }

        [Required]
        public string Username { get; set; }

        public string Email { get; set; }

        public string Picture { get; set; }

        public string TeamId { get; set; }
        public Team Team { get; set; }
    }

}