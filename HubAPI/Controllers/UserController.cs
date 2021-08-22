using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HubBL;
using HubEntities.Database;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Auth0.AuthenticationApi;
using Microsoft.Extensions.Configuration;
using Auth0.AuthenticationApi.Models;

namespace HubAPI.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase {
        private readonly UserManager _userManager;
        private readonly IConfiguration _configuration;
        public UserController(IConfiguration configuration, UserManager userManager) {
            _userManager = userManager;
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet("login")]
        public async Task<IActionResult> Login() {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (userId == null) BadRequest();

            User targetUser = await _userManager.GetUser(userId);

            if (targetUser == null) {
                string accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token").Value;
                AuthenticationApiClient apiClient = new AuthenticationApiClient(_configuration["auth0:domain"]);
                UserInfo userInfo = await apiClient.GetUserInfoAsync(accessToken);

                targetUser = await _userManager.CreateUser(new() {
                    Id = userId
                });
            }

            //Returns the user if they exist or creates a new database entry if it doesn't
            return Ok(targetUser);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditProfile([FromBody] User p_user) {
            return Ok(await _userManager.EditProfile(p_user));
        }
    }
}
