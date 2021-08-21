using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HubEntities.Database {
    public class TeamJoinRequest {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public string TeamName { get; set; }

        public string UserId { get; set; }

        public User User { get; set; }
    }
}