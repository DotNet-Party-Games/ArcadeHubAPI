using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HubBL;
using HubEntities.Database;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HubAPI.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class TeamController : ControllerBase {
        private readonly TeamManager _teamManager;
        private readonly ILogger<TeamController> _logger;
        public TeamController(TeamManager teamManager, ILogger<TeamController> logger) {
            _logger = logger;
            _teamManager = teamManager;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllTeams() {
            return Ok();
        }

        [HttpGet("request/{teamName}")]
        public async Task<IActionResult> GetAllRequests() {
            return Ok();
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateTeam([FromBody] Team team) {
            if (!ModelState.IsValid) {
                _logger.LogError("[TEAM: CreateTeam] Invalid team format.");
                return BadRequest("Team is not in a valid format");
            }

            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (userId == null) {
                _logger.LogError("[TEAM: CreateTeam] Unable to load userId from JWT.");
                return BadRequest("Token error");
            }
            
            return Ok(await _teamManager.CreateTeam(team.Name, team.Description, userId));
        }

        [Authorize]
        [HttpPut("request/{teamName}")]
        public async Task<IActionResult> JoinRequest([FromRoute]string teamName) {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (userId == null) {
                _logger.LogError("[TEAM: JoinRequest] Unable to load userId from JWT.");
                return BadRequest("Token error");
            }

            return Ok(await _teamManager.CreateRequest(userId, teamName));
        }


        [Authorize]
        [HttpPut("request/{requestId}")]
        public async Task<IActionResult> JoinDecision(string requestId, bool approve=true) {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (userId == null) {
                _logger.LogError("[TEAM: CreateTeam] Unable to load userId from JWT.");
                return BadRequest("Token error");
            }

            return Ok(await _teamManager.ApproveOrDenyRequest(requestId, userId, approve));
        }

        [Authorize]
        [HttpPut("leave/{p_userEmail}/{p_teamName}")]
        public async Task<IActionResult> LeaveTeam(string p_userEmail, string p_teamName) {
            return Ok(await _teamManager.LeaveTeam(p_userEmail, p_teamName));
        }

        [Authorize]
        [HttpDelete("disband/{p_teamOwner}/{p_teamName}")]
        public async Task<IActionResult> DisbandTeam([FromRoute] string p_teamOwner, string p_teamName) {
            return Ok(await _teamManager.DisbandTeam(p_teamOwner, p_teamName));
        }
    }
}
