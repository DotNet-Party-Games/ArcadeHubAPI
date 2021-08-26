using HubEntities.Database;
using System.ComponentModel.DataAnnotations;

namespace HubEntities.Dto {
    public class ChatStatusDto {
        [Required]
        public UserDto User { get; set; }

        [Required]
        public string Status { get; set; }
    }
}