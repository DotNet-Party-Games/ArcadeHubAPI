using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HubBL;
using Microsoft.Extensions.Logging;
using HubEntities.Database;
using HubEntities.Dto;

namespace HubAPI.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class LeaderboardController : ControllerBase {
        private readonly LeaderboardManager _leaderboardManager;
        private readonly ILogger<LeaderboardController> _logger;

        public LeaderboardController(LeaderboardManager leaderboardManager, ILogger<LeaderboardController> logger) {
            _leaderboardManager = leaderboardManager;
            _logger = logger;
        }

        [HttpGet("individual/{gameType}")]
        public async Task<IActionResult> GetIndividualLeaderboard([FromRoute] string gameType) {
            return Ok(await _leaderboardManager.GetLeaderboard(gameType));
        }

        [HttpPost("individual/{gameType}")]
        public async Task<IActionResult> SubmitScore([FromRoute] string gameType, [FromBody] UserScore score) {
            Leaderboard leaderboard = await _leaderboardManager.SubmitScore(gameType, score);
            if (leaderboard != null) {
                _logger.LogInformation("[LEADERBOARD: SubmitScore] User Score successfully submitted for game type '{gameType}'.", gameType);
            }
            return Ok(leaderboard);
        }

        [HttpGet("team/{gameType}")]
        public async Task<IActionResult> GetTeamLeaderboard([FromRoute] string gameType) {
            TeamLeaderboard teamLeaderboard = await _leaderboardManager.GetTeamLeaderboard(gameType);
            if (teamLeaderboard != null) {
                _logger.LogInformation("[LEADERBOARD: GetTeamLeaderboard] Query for team leaderboard for game type '{gameType}' was successful.", gameType);
            }
            return Ok(teamLeaderboard);
        }

        [HttpPost("team/{gameType}")]
        public async Task<IActionResult> SubmitTeamScore([FromRoute] string gameType, [FromBody] TeamScoreDto score) {
            TeamLeaderboard teamLeaderboard = await _leaderboardManager.SubmitTeamScore(gameType, new TeamScore {
                TeamName = score.TeamName,
                Score = score.Score
            });
            if (teamLeaderboard != null) {
                _logger.LogInformation("[LEADERBOARD: SubmitTeamScore] Team Score successfully submitted for game type '{gameType}'.", gameType);
            }
            return Ok(teamLeaderboard);
        }
    }
}
