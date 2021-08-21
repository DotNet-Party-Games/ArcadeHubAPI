using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HubEntities.Database {
    public class Team {
        public Team() {
            Users = new HashSet<User>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public string Name { get; set; }

        public string TeamOwner { get; set; }

        public string Description { get; set; }

        public ICollection<User> Users { get; set; }
    }

}