using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HubEntities.Database {
    public class UserScore {
        [Key]
        public string UserEmail { get; set; }

        public int Score { get; set; }
    }

}