using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HubEntities.Database {
    public class TeamLeaderboard {
        public TeamLeaderboard() {
            Scores = new HashSet<TeamScore>();
        }

        [Key]
        public string Id { get; set; }

        public ICollection<TeamScore> Scores { get; set; }
    }

}