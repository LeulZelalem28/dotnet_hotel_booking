using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;


namespace User.Service.Controllers
{
    [ApiController]
    [Route("logout")]
    public class LogoutController : ControllerBase
    {
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> LogoutAsync()
        {
            // Perform the logout logic
            await HttpContext.SignOutAsync(); // Sign out the current user

            return Ok("Logged out successfully");
        }
    }
}