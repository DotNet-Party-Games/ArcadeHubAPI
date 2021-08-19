using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HubBL;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserBL _UserBL;
        public UserController(IUserBL p_UserBL)
        {
            _UserBL = p_UserBL;
        }

        //This return User object if exist
        [HttpGet("login/{p_email}")]
        public async Task<IActionResult> LogIn(string p_email)
        {
            
        }

        [HttpPut("editProfile")]
        public async Task<IActionResult> EditProfile([FromBody] User p_user)
        {
            return Ok(await _UserBL.EditProfile(p_user));
        }
    }
}
