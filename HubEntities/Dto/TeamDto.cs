using HubEntities.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubEntities.Dto {
    public class TeamDto {
        public string Id { get; set; }

        public string Name { get; set; }

        public string TeamOwner { get; set; }

        public string Description { get; set; }

        public ICollection<TeamMemberDto> Users { get; set; }
    }
}
