using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HubEntities.Database {
    public class TeamJoinRequest {
        [Key]
        public string Id { get; set; }

        public string TeamName { get; set; }

        public int UserId { get; set; }
    }

}