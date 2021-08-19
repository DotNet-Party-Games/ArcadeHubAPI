using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HubEntities.Database {
    public class Leaderboard {
        [Key]
        public string Id { get; set; }

        public List<UserScore> Scores;
    }

}