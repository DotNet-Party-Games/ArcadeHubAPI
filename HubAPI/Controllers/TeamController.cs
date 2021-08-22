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
            IList<Team> teams = await _teamManager.GetAllTeams();
            if (teams != null) {
                _logger.LogInformation($"[TEAM: GetAllTeams] Query for all teams returned {teams.Count} results.");
            }
            return Ok(teams);
        }

        [HttpGet("{teamName}")]
        public async Task<IActionResult> GetAllTeams([FromRoute] string teamName) {
            Team team = await _teamManager.GetTeamByName(teamName);
            if (team != null) {
                _logger.LogInformation($"[TEAM: GetAllTeams] Query for team with name \"{teamName}\" successful.");
            } else {
                _logger.LogInformation($"[TEAM: GetAllTeams] Query for team with name \"{teamName}\" returned no result.");
            }
            return Ok(team);
        }


        [HttpGet("request/{teamId}")]
        public async Task<IActionResult> GetAllRequests(string teamId) {
            IList<TeamJoinRequest> requests = await _teamManager.GetRequestsByTeamName(teamId);
            if (requests != null) {
                _logger.LogInformation($"[TEAM: GetAllRequests] Query for team join requests for team \"{teamId}\" returned {requests.Count} results.");
            }

            return Ok(requests);
        }

        [Authorize]
        [HttpPost()]
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

            Team newTeam = await _teamManager.CreateTeam(team.Name, team.Description, userId);
            if (newTeam != null) {
                _logger.LogInformation($"[TEAM: CreateTeam] New team with name \"{newTeam.Name}\" has been created.");
            }

            return Ok(newTeam);
        }

        [Authorize]
        [HttpPut("request/{teamName}")]
        public async Task<IActionResult> JoinRequest([FromRoute]string teamName) {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (userId == null) {
                _logger.LogError("[TEAM: JoinRequest] Unable to load userId from JWT.");
                return BadRequest("Token error");
            }

            TeamJoinRequest newRequest = await _teamManager.CreateRequest(userId, teamName);
            if (newRequest != null) {
                _logger.LogInformation($"[TEAM: JoinRequest] Request by user with ID \"{newRequest.UserId}\" to join team \"{newRequest.TeamName}\"has been created.");
            }

            return Ok(newRequest);
        }


        [Authorize]
        [HttpPut("request/{requestId}")]
        public async Task<IActionResult> ApproveOrDenyRequest(string requestId, bool approve=true) {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (userId == null) {
                _logger.LogError("[TEAM: ApproveOrDenyRequest] Unable to load userId from JWT.");
                return BadRequest("Token error");
            }

            bool results = await _teamManager.ApproveOrDenyRequest(requestId, userId, approve);

            if (approve && results) {
                _logger.LogInformation($"[TEAM: ApproveOrDenyRequest] Request with ID \"{requestId}\" has been approved.");
            } else {
                _logger.LogInformation($"[TEAM: ApproveOrDenyRequest] Request with ID \"{requestId}\" has been denied.");
            }
            return Ok(results);
        }

        [Authorize]
        [HttpPut("leave")]
        public async Task<IActionResult> LeaveTeam() {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (userId == null) {
                _logger.LogError("[TEAM: LeaveTeam] Unable to load userId from JWT.");
                return BadRequest("Token error");
            }

            bool results = await _teamManager.LeaveTeam(userId);

            if (results) {
                _logger.LogInformation($"[TEAM: LeaveTeam] User with ID \"{userId}\" has successfully left their team.");
            }

            return Ok(results);
        }

        [Authorize]
        [HttpDelete("{teamName}")]
        public async Task<IActionResult> DisbandTeam([FromRoute] string teamName) {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (userId == null) {
                _logger.LogError("[TEAM: LeaveTeam] Unable to load userId from JWT.");
                return BadRequest("Token error");
            }

            bool result = await _teamManager.DisbandTeam(userId, teamName);
            if (result) {
                _logger.LogInformation($"[TEAM: DisbandTeam] Team \"{teamName}\" has been disbanded by its owner.");
            }
            return Ok(result);
        }
    }
}
