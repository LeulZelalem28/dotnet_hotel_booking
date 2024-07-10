using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq; // Added using statement
using Common;
using System.Threading.Tasks;
using User.Service.Models;
using User.Service.Dtos;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace User.Service.Controllers;

    [ApiController]
    [Route("users")]
    public class SignupController : ControllerBase
    {
        private readonly IService<Person> _userService;
        ILogger<SignupController> _logger;
        public SignupController(IService<Person> userService, ILogger<SignupController> logger){
            _userService = userService;
            _logger = logger;
        }
        [HttpPost("signup")]
        public async Task<ActionResult<UserDto>> PostAsync(CreateUserDto createUserDto){
            try{
                //check if all the fields are inserted
            if(string.IsNullOrEmpty(createUserDto.Username) ||
               string.IsNullOrEmpty(createUserDto.Email) ||
               string.IsNullOrEmpty(createUserDto.Password) ||
               string.IsNullOrEmpty(createUserDto.PhoneNumber))
               //if not return badrequest error message
            {
                return BadRequest("The required fields are missing");
            }
            //checking if username or email already exists
            var userByUsername = await _userService.GetAsync(user => user.Username == createUserDto.Username);
            var usernameAlreadyExists = userByUsername?.Username;
            var userByEmail = await _userService.GetAsync(user => user.Email == createUserDto.Email);
            var emailAlreadyExists = userByEmail?.Email;
            
            //checking if username exists
            if(usernameAlreadyExists != null)
            {
                return Conflict("Username already exists");
            }

            //checking if email exists
            if(emailAlreadyExists != null)
            {
                return Conflict("Email already exists");
            }
            //create a new person using the dto sent from user
            var user = new Person{
               Id = Guid.NewGuid(),
               Username = createUserDto.Username,  
               Email = createUserDto.Email,  
               Password = EncryptPassword(createUserDto.Password),  
               PhoneNumber = createUserDto.PhoneNumber,
               CreatedDate = DateTimeOffset.UtcNow
               };
               //add in to the db
               await _userService.CreateAsync(user);
               //return status code created returning the user created
               return CreatedAtAction(nameof(GetUserAsync), new { id = user.Id }, user.AsDto());
            }catch (Exception ex){
                // Log the error 
                  _logger.LogError(ex, "An error occurred while processing the request.");

                // Return an error to user
                return StatusCode(500, "An error occurred while processing the request. Please try again later.");
            }
    }
        private static string EncryptPassword(string password)
    {
        // Generate a random salt
        string salt = BCrypt.Net.BCrypt.GenerateSalt(10);
       // Hash the password with the generated salt
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);
        return hashedPassword;
    }
    [ActionName(nameof(GetUserAsync))]
        [HttpGet("{id}")]
        
        public async Task <ActionResult<UserDto>> GetUserAsync(Guid id)
        {
            var user = await _userService.GetAsync(id);
            if(user == null){
                return NotFound();
            }
            return Ok(user.AsDto());
        }
    }