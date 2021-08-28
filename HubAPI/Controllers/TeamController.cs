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
using HubEntities.Dto;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace HubAPI.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class TeamController : ControllerBase {
        private readonly TeamManager _teamManager;
        private readonly UserManager _userManager;
        private readonly ILogger<TeamController> _logger;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubcontext;
        public TeamController(
            TeamManager teamManager,
            UserManager userManager,
            ILogger<TeamController> logger, 
            IMapper mapper, 
            IHubContext<ChatHub> hubcontext) {
            _logger = logger;
            _teamManager = teamManager;
            _userManager = userManager;
            _mapper = mapper;
            _hubcontext = hubcontext;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllTeams() {
            IList<Team> teams = await _teamManager.GetAllTeams();
            if (teams != null) {
                _logger.LogInformation("[TEAM: GetAllTeams] Query for all teams returned {count} results.", teams.Count);
            }
            return Ok(_mapper.Map<IList<Team>, IList<TeamDto>>(teams));
        }

        [HttpGet("{teamName}")]
        public async Task<IActionResult> GetTeam([FromRoute] string teamName) {
            Team team = await _teamManager.GetTeamByName(teamName);
            if (team != null) {
                _logger.LogInformation("[TEAM: GetTeam] Query for team with name '{teamName}' successful.", teamName);
            } else {
                _logger.LogInformation("[TEAM: GetTeam] Query for team with name '{teamName}' returned no result.", teamName);
                return BadRequest(new { error = $"No team found with name '{teamName}'" });
            } 
            return Ok(_mapper.Map<TeamDto>(team));
        }


        [HttpGet("request/{teamName}")]
        public async Task<IActionResult> GetAllRequests([FromRoute]string teamName) {
            IList<TeamJoinRequest> requests = await _teamManager.GetRequestsByTeamName(teamName);
            if (requests != null) {
                _logger.LogInformation("[TEAM: GetAllRequests] Query for team join requests for team '{teamName}' returned {count} results.", teamName, requests.Count);
            }

            return Ok(requests);
        }

        [Authorize]
        [HttpPost()]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto team) {
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
                _logger.LogInformation("[TEAM: CreateTeam] New team with name '{name}' has been created.", newTeam.Name);
            }

            return Ok(_mapper.Map<TeamDto>(newTeam));
        }

        [Authorize]
        [HttpPut("request/{teamName}")]
        public async Task<IActionResult> JoinRequest([FromRoute] string teamName) {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (userId == null) {
                _logger.LogError("[TEAM: JoinRequest] Unable to load userId from JWT.");
                return BadRequest("Token error");
            }

            User user = await _userManager.GetUser(userId);
            if (user == null) {
                _logger.LogError("[TEAM: JoinRequest] Unable to load user with Id {userId} from JWT.", userId);
                return BadRequest("Token error");
            }

            TeamJoinRequest newRequest = await _teamManager.CreateRequest(userId, teamName);
            if (newRequest != null) {
                _logger.LogInformation("[TEAM: JoinRequest] Request by user with ID '{userId}' to join team '{teamName}' has been created.", newRequest.UserId, newRequest.TeamName);
            }

            // Alert Team owner if online
            Team team = await _teamManager.GetTeamByName(teamName);
            if (team != null) {
                User teamOwner = await _userManager.GetUser(team.TeamOwner);
                if (teamOwner != null && teamOwner.Connections != null) {
                    foreach (ChatConnection c in teamOwner.Connections) {
                        _logger.LogError("[TEAM: JoinRequest] Alert sent to team owner with ID {teamOwnerId}'.", teamOwner.Id);
                        await _hubcontext.Clients.Client(c.ConnectionId).SendAsync("Alert", new AlertDto { 
                            AlertType = "NEW TEAM JOIN REQUEST",
                            Message = $"{user.Username} has requested to join your team '{team.Name}'"
                        });
                    }
                }
            } else {
                _logger.LogError("[TEAM: JoinRequest] Unable to load team with name '{teamName}'.", teamName);
            }

            return Ok(_mapper.Map<TeamJoinRequestDto>(newRequest));
        }


        [Authorize]
        [HttpPut("request/approve/{requestId}")]
        public async Task<IActionResult> ApproveOrDenyRequest(string requestId, bool approve=true) {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (userId == null) {
                _logger.LogError("[TEAM: ApproveOrDenyRequest] Unable to load userId from JWT.");
                return BadRequest("Token error");
            }

            TeamJoinRequest result = await _teamManager.ApproveOrDenyRequest(requestId, userId, approve);

            if (result == null) {
                _logger.LogError("[TEAM: ApproveOrDenyRequest] Error processing request with ID: {requestId}", requestId);
                return BadRequest($"Error processing request with ID: {requestId}");
            }

            User targetUser = await _userManager.GetUser(result.UserId);

            // Notify User that their request has been processed.
            if (targetUser != null && targetUser.Connections != null) {
                foreach (ChatConnection c in targetUser.Connections) {
                    _logger.LogError("[TEAM: ApproveOrDenyRequest] Alert sent to team owner with ID {userId}'.", targetUser.Id);
                    await _hubcontext.Clients.Client(c.ConnectionId).SendAsync("Alert", new AlertDto {
                        AlertType = (approve) ? "REQUEST APPROVED" : "REQUEST DENIED",
                        Message = (approve) 
                            ? $"Your request to join team '{result.TeamName}' has been approved"
                            : $"Your request to join team '{result.TeamName}' has been denied"
                    });
                }
            }

            if (approve) {
                _logger.LogInformation("[TEAM: ApproveOrDenyRequest] Request with ID '{requestId}' has been approved.", requestId);
                // Notify team members
                await NotifyTeam(result.TeamName, "USER JOINED TEAM", $"{targetUser.Username} has joined your team '{result.TeamName}'");


            } else {
                _logger.LogInformation("[TEAM: ApproveOrDenyRequest] Request with ID '{requestId}' has been denied.", requestId);
            }
            return Ok(_mapper.Map<TeamJoinRequestDto>(result));
        }


        [Authorize]
        [HttpPut("leave")]
        public async Task<IActionResult> LeaveTeam() {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (userId == null) {
                _logger.LogError("[TEAM: LeaveTeam] Unable to load userId from JWT.");
                return BadRequest("Token error");
            }

            (Team targetTeam, User targetUser) = await _teamManager.LeaveTeam(userId);

            if (targetTeam != null && targetUser != null) {
                _logger.LogInformation("[TEAM: LeaveTeam] User with ID '{userId}' has successfully left their team.", userId);
                //Notify all team members
                await NotifyTeam(targetTeam.Name, "USER LEFT TEAM", $"{targetUser.Username} has left your team '{targetTeam.Name}'");
            }

            return Ok();
        }

        [Authorize]
        [HttpDelete("{teamName}")]
        public async Task<IActionResult> DisbandTeam([FromRoute] string teamName) {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (userId == null) {
                _logger.LogError("[TEAM: DisbandTeam] Unable to load userId from JWT.");
                return BadRequest("Token error");
            }

            Team targetTeam = await _teamManager.DisbandTeam(userId, teamName);
            if (targetTeam != null) {
                _logger.LogInformation("[TEAM: DisbandTeam] Team '{teamName}' has been disbanded by its owner.", teamName);
                // Notify all team members
                await NotifyTeam(targetTeam, "TEAM DISBANDED", $"Your team '{targetTeam.Name}' has been disbanded by its owner");

            }
            return Ok();
        }

        private async Task NotifyTeam(string teamName, string alertType, string message) {
            Team targetTeam = await _teamManager.GetTeamByName(teamName);

            if (targetTeam == null || targetTeam.Users == null) return;
            foreach (User user in targetTeam.Users) {
                if (user.Connections == null) continue;
                foreach (ChatConnection connection in user.Connections) {
                    await _hubcontext.Clients.Client(connection.ConnectionId).SendAsync("Alert", new AlertDto {
                        AlertType = alertType,
                        Message = message
                    });
                }
            }
        }

        private async Task NotifyTeam(Team targetTeam, string alertType, string message) {
            if (targetTeam == null || targetTeam.Users == null) return;
            foreach (User user in targetTeam.Users) {
                if (user.Connections == null) continue;
                foreach (ChatConnection connection in user.Connections) {
                    await _hubcontext.Clients.Client(connection.ConnectionId).SendAsync("Alert", new AlertDto {
                        AlertType = alertType,
                        Message = message
                    });
                }
            }
        }
    }
}
