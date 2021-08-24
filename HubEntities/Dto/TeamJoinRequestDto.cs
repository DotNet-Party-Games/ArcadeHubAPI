using System.ComponentModel.DataAnnotations;


namespace HubEntities.Dto {
    public class TeamJoinRequestDto {
        [Key]
        public string Id { get; set; }

        public string TeamName { get; set; }

        public string UserId { get; set; }

        public UserDto User { get; set; }
    }
}
