using System.ComponentModel.DataAnnotations;

namespace HubEntities.Dto {
    public class CreateTeamDto {

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }
    }
}