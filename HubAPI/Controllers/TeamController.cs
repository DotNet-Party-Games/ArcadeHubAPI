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
        public async Task<IActionResult> CreateTeam([FromBody] Team team)
        {
            //return Ok(await _teamManager.CreateTeam(team));
            return Ok();
        }

        [HttpPut("join/{p_userEmail}/{p_teamName}")]
        public async Task<IActionResult> JoinRequest(string p_userEmail, string p_teamName)
        {
            return Ok(await _teamManager.CreateRequest(p_userEmail, p_teamName));
        }


        //Not sure about this method
        [HttpPut("joinDecision/{p_requestId}/{p_approve}")]
        public async Task<IActionResult> JoinDecision(string p_requestId, bool p_approve)
        {
            return Ok(await _teamManager.ApproveOrDenyRequest(p_requestId, p_approve));
        }

        [HttpPut("leave/{p_userEmail}/{p_teamName}")]
        public async Task<IActionResult> LeaveTeam(string p_userEmail, string p_teamName)
        {
            return Ok(await _teamManager.LeaveTeam(p_userEmail, p_teamName));
        }

        [HttpDelete("disband/{p_teamOwner}/{p_teamName}")]
        public async Task<IActionResult> DisbandTeam([FromRoute] string p_teamOwner, string p_teamName )
        {
            return Ok(await _teamManager.DisbandTeam(p_teamOwner, p_teamName));
        }
    }
}
