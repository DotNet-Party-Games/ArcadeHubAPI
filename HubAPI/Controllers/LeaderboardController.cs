using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HubBL;
using Microsoft.Extensions.Logging;
using HubEntities.Database;

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

        [HttpGet("leaderboard/{gameType}")]
        public async Task<IActionResult> GetIndividualLeaderboard([FromRoute] string gameType) {
            return Ok(await _leaderboardManager.GetLeaderboard(gameType));
        }

        [HttpPost("teamleaderboard/{gameType}")]
        public async Task<IActionResult> SubmitScore([FromRoute] string gameType, [FromBody] UserScore score) {
            Leaderboard leaderboard = await _leaderboardManager.SubmitScore(gameType, score);
            if (leaderboard != null) {
                _logger.LogInformation($"[LEADERBOARD: SubmitScore] User Score successfully submitted for game type \"{gameType}\".");
            }
            return Ok(leaderboard);
        }

        [HttpGet("teamleaderboard/{gameType}")]
        public async Task<IActionResult> GetTeamLeaderboard([FromRoute] string gameType) {
            TeamLeaderboard teamLeaderboard = await _leaderboardManager.GetTeamLeaderboard(gameType);
            if (teamLeaderboard != null) {
                _logger.LogInformation($"[LEADERBOARD: GetTeamLeaderboard] Query for team leaderboard for game type \"{gameType}\" was successful.");
            }
            return Ok(teamLeaderboard);
        }

        [HttpPost("teamleaderboard/{gameType}")]
        public async Task<IActionResult> SubmitTeamScore([FromRoute] string gameType, [FromBody] TeamScore score) {
            TeamLeaderboard teamLeaderboard = await _leaderboardManager.SubmitTeamScore( gameType, score);
            if (teamLeaderboard != null) {
                _logger.LogInformation($"[LEADERBOARD: SubmitTeamScore] Team Score successfully submitted for game type \"{gameType}\".");
            }
            return Ok(teamLeaderboard);
        }
    }
}
