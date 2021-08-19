using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HubEntities.Database {
    public class Leaderboard {
        public Leaderboard() {
            Scores = new HashSet<UserScore>();
        }

        [Key]
        public string Id { get; set; }

        public ICollection<UserScore> Scores;
    }

}