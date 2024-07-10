using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq; // Added using statement
using Common;
using System.Threading.Tasks;
using User.Service.Models;
using User.Service.Dtos;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Serializers;

namespace User.Service.Controllers;
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly IService<Person> _userService;
        ILogger<UsersController> _logger;
        public UsersController(IService<Person> userService, ILogger<UsersController> logger){
            _userService = userService;
            _logger = logger;
        }
         [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersAsync()
        {
            var users = (await _userService.GetAllAsync()).Select(users => users.AsDto());
            return Ok(users);
        }
        
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto){
            var existingUser = await _userService.GetAsync(id);

            // checking if user exists
            if(existingUser == null){
                return NotFound();
            }   
            
           
            //optional editing
            if(updateUserDto.Username != null)
            {
            var userByUsername = await _userService.GetAsync(user => user.Username == updateUserDto.Username);
            var usernameAlreadyExists = userByUsername?.Username;
                //checking if username exists
            if(usernameAlreadyExists != null)
            {
                return Conflict("Username already exists");
            }
                existingUser.Username = updateUserDto.Username;
               }

            if(updateUserDto.Email != null)
               {
                 var userByEmail = await _userService.GetAsync(user => user.Email == updateUserDto.Email);
            var emailAlreadyExists = userByEmail?.Email;
            //checking if email exists
            if(emailAlreadyExists != null)
            {
                return Conflict("Email already exists");
            }
               existingUser.Email = updateUserDto.Email;
               }

               
               if(updateUserDto.PhoneNumber != null)
               {
               existingUser.PhoneNumber = updateUserDto.PhoneNumber;
               }


            // Checking if password is being edited
            if (!string.IsNullOrEmpty(updateUserDto.Password))
            {
               existingUser.Password = EncryptPassword(updateUserDto.Password);
            }

            await _userService.UpdateAsync(existingUser);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAsync(Guid id){
             var user = await _userService.GetAsync(id);
            if(user == null){
                return NotFound();
            }   
             await _userService.RemoveAsync(user.Id);
             return NoContent();
        }

        private static string EncryptPassword(string password)
    {
        // Generate a random salt
        string salt = BCrypt.Net.BCrypt.GenerateSalt(10);
       // Hash the password with the generated salt
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

        return hashedPassword;
    }
    }
