using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq; // Added using statement
using Common;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using User.Service.Models;
using User.Service.Dtos;
using BCrypt.Net;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace User.Service.Controllers;

    [ApiController]
    [Route("login")]
    public class LoginController : ControllerBase
    {
        private readonly IService<Person> _userService;
        ILogger<LoginController> _logger;
        private IConfiguration _config;
        public LoginController(IService<Person> userService, IConfiguration config, ILogger<LoginController> logger){
            _userService = userService;
            _logger = logger;
            _config = config;
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<UserDto>> LoginAsync(LoginDto loginDto){
            var userFound = await _userService.GetAsync(user => user.Username == loginDto.Username);
            //checking if user by provided username exists
            if(userFound == null)
            {
                return NotFound("User not found");
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password,userFound.Password);
            if(!isPasswordValid)
            {
                return BadRequest("Invalid Password");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
             var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFound.Username),
                new Claim(ClaimTypes.Email, userFound.Email),
                new Claim(ClaimTypes.Role, userFound.Role),
                new Claim("UserId", userFound.Id.ToString())
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Audience"],
              claims,
              expires: DateTime.Now.AddHours(24),
              signingCredentials: credentials);

            var AccessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(AccessToken);
        }
    }           