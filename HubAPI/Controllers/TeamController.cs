using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HubBL;
using HubEntities.Database;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly TeamManager _teamManager;
        public TeamController(TeamManager teamManager)
        {
            _teamManager = teamManager;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTeam(string p_userEmail, [FromBody] Team team)
        {
            return Ok();
        }

        [HttpPut("join")]
        public async Task<IActionResult> JoinTeam(string p_userEmail, string p_teamName)
        {
            return Ok();
        }

        //public async Task<IActionResult> JoinRequest()
        //{

        //}

        [HttpPut("leave")]
        public async Task<IActionResult> LeaveTeam(string p_userEmail, string p_teamName)
        {
            return Ok();
        }

        [HttpDelete("disband/{p_teamName}")]
        public async Task<IActionResult> DisbandTeam([FromRoute] string p_teamName)
        {
            return Ok();
        }
    }
}
