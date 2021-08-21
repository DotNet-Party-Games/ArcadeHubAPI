using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HubEntities.Database {
    public class User {
        public User() {
            Teams = new HashSet<Team>();
        }

        [Key]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        public string ProfileImage { get; set; }

        public ICollection <Team> Teams { get; set; }
    }

}