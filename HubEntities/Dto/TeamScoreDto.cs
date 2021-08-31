using HubEntities.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubEntities.Dto {
    public class TeamScoreDto {

        [Required]
        public string TeamName { get; set; }
        public int Score { get; set; } = 0;
    }
}
