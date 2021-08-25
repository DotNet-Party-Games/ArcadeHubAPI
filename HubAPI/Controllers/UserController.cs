using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HubBL;
using HubEntities.Database;
using HubEntities.Dto;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Auth0.AuthenticationApi;
using Microsoft.Extensions.Configuration;
using Auth0.AuthenticationApi.Models;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace HubAPI.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase {
        private readonly UserManager _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;
        private readonly IMapper _mapper;
        public UserController(
            IConfiguration configuration, 
            UserManager userManager, 
            ILogger<UserController> logger,
            IMapper mapper) {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
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
                _logger.LogInformation("[USER: GetUser] New user with ID '{userId}' is being created.", userId);
                string accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token").Value;
                AuthenticationApiClient apiClient = new(_configuration["auth0:domain"]);
                UserInfo userInfo = await apiClient.GetUserInfoAsync(accessToken);

                targetUser = await _userManager.CreateUser(new() {
                    Id = userId,
                    Email = userInfo.Email,
                    Username = userInfo.NickName,
                    Picture = userInfo.Picture
                });
                _logger.LogInformation("[USER: GetUser] New user with ID '{userId}' successfully created.", userId);
            }

            return Ok(_mapper.Map<UserDto>(targetUser));
            
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditProfile([FromBody] EditUserDto user) {
            if (!ModelState.IsValid) {
                _logger.LogError("[USER: EditProfile] Invalid user profile format.");
                return BadRequest("User is not in a valid format");
            }
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (userId == null) {
                _logger.LogError("[USER: EditProfile] Unable to load userId from JWT.");
                return BadRequest("Token error");
            }

            User targetUser = _mapper.Map<User>(user);
            targetUser.Id = userId;

            User editedUser = await _userManager.EditProfile(targetUser);

            if (editedUser == null) {
                _logger.LogError("[USER: EditProfile] Failed to edit user.");
                return BadRequest("Error editing user");
            }
            _logger.LogInformation("[USER: EditProfile] User with ID '{userId}' successfully modified their account.", userId);
            return Ok(editedUser);
        }
    }
}
