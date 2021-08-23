using HubEntities.Database;
using System.ComponentModel.DataAnnotations;

namespace HubEntities.Dto {
    public class ChatStatusDto {

        [Required]
        public User User { get; set; }

        [Required]
        public string Status { get; set; }

    }
}