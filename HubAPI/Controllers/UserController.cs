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
    public class UserController : ControllerBase
    {
        private readonly UserManager _userManager;
        public UserController(UserManager userManager)
        {
            _userManager = userManager;
        }

        //This return User object if exist
        [HttpGet("login/{p_email}")]
        public async Task<IActionResult> LogIn(string p_email)
        {
            return Ok();
        }

        [HttpPut("editProfile")]
        public async Task<IActionResult> EditProfile([FromBody] User p_user)
        {
            return Ok(await _userManager.EditProfile(p_user));
        }
    }
}
