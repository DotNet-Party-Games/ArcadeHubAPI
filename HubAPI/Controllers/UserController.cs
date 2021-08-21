using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HubBL;
using HubEntities.Database;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace HubAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase {
        private readonly UserManager _userManager;
        public UserController(UserManager userManager) {
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet("login")]
        public async Task<IActionResult> Login() {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            string email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            User targetUser = await _userManager.GetUser(email);
            
            //Returns the user if they exist or creates a new database entry if it doesn't
            return Ok(targetUser ?? await _userManager.CreateUser(new() { 
                Email = email,
                Username = email,
                ProfileImage = User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value
            }));
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditProfile([FromBody] User p_user) {
            return Ok(await _userManager.EditProfile(p_user));
        }
    }
}
