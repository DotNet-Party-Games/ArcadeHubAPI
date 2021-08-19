using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HubEntities.Database {
    public class Team {
        [Key]
        public string Name { get; set; }

        public string Description { get; set; }

        public List<User> Users { get; set; }
    }

}