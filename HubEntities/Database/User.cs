using System;
using System.ComponentModel.DataAnnotations;

namespace HubEntities.Database {
    public class User {
        [Key]
        public string Id { get; set; }
    }

}