using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HubBL;

namespace HubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaderboardController : ControllerBase
    {
        private readonly LeaderboardManager _leaderboardManager;

        public LeaderboardController(LeaderboardManager leaderboardManager)
        {
            _leaderboardManager = leaderboardManager;
        }

        [HttpGet("getIndividualLeaderboard/{p_gameType}")]
        public async Task<IActionResult> GetIndividualLeaderboard([FromRoute] string p_gameType)
        {
            return Ok();
        }

        [HttpGet("getTeamLeaderboard/{p_gameType}")]
        public async Task<IActionResult> GetTeamLeaderboard([FromRoute]string p_gameType)
        {
            return Ok();
        }
    }
}
