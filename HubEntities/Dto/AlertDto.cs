using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubEntities.Dto {
    public class AlertDto {

        [Required]
        public string Message { get; set; }

        [Required]
        public string AlertType { get; set; }
    }
}
