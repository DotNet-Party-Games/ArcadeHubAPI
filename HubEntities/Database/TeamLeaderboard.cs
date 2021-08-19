using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HubEntities.Database {
    public class TeamLeaderboard {
        [Key]
        public string Id { get; set; }

        public List<TeamScore> Scores;
    }

}