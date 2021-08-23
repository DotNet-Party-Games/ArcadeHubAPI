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
using Microsoft.Extensions.Logging;

namespace HubAPI.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase {
        private readonly UserManager _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;
        public UserController(IConfiguration configuration, UserManager userManager, ILogger<UserController> logger) {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUser() {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (userId == null) {
                _logger.LogError("[USER: GetUser] Unable to load userId from JWT.");
                return BadRequest("Token error"); 
            }

            User targetUser = await _userManager.GetUser(userId);

            if (targetUser == null) {
                _logger.LogInformation($"[USER: GetUser] New user with ID \"{userId}\" is being created.");
                string accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token").Value;
                AuthenticationApiClient apiClient = new(_configuration["auth0:domain"]);
                UserInfo userInfo = await apiClient.GetUserInfoAsync(accessToken);

                targetUser = await _userManager.CreateUser(new() {
                    Id = userId,
                    Email = userInfo.Email,
                    Username = userInfo.NickName,
                    Picture = userInfo.Picture
                });
                _logger.LogInformation($"[USER: GetUser] New user with ID \"{userId}\" successfully created.");
            }

            return Ok(targetUser);
            
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditProfile([FromBody] User user) {
            if (!ModelState.IsValid) {
                _logger.LogError("[USER: EditProfile] Invalid user profile format.");
                return BadRequest("User is not in a valid format");
            }
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (userId == null) {
                _logger.LogError("[USER: GetUser] Unable to load userId from JWT.");
                return BadRequest("Token error");
            }
            if (userId != user.Id) {
                _logger.LogError($"[USER: GetUser] User with ID \"{userId}\" attempted to modify an account with ID \"{user.Id}\".");
                return BadRequest($"You are not authorized to modify this account with ID: \"{user.Id}\".");
            }

            _logger.LogInformation($"[USER: GetUser] User with ID \"{userId}\" successfully modified their account.");
            return Ok(await _userManager.EditProfile(user));
        }
    }
}
