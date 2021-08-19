using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardBL _LeaderboardBL;
        private readonly ITeamLeaderboardBL _TeamLeaderboardBL;

        public LeaderboardController(ILeaderboardBL p_LeaderboardBL, ITeamLeaderboardBL p_TeamLeaderboardBL)
        {
            _LeaderboardBL = p_LeaderboardBL;
            _TeamLeaderboardBL = p_TeamLeaderboardBL;
        }

        [HttpGet("getIndividualLeaderboard/{p_gameType}")]
        public async Task<IActionResult> GetIndividualLeaderboard(string p_gameType)
        {

        }

        [HttpGet("getTeamLeaderboard/{p_gameType}")]
        public async Task<IActionResult> GetTeamLeaderboard(string p_gameType)
        {

        }
    }
}
