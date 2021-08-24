using HubEntities.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubEntities.Dto {
    public class TeamMemberDto {
        [Key]
        public string Id { get; set; }

        [Required]
        public string Username { get; set; }

        public string Email { get; set; }

        public string Picture { get; set; }
    }
}
