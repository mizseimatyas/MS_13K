using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebShop.Dto;
using WebShop.Model;

namespace WebShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminsController : ControllerBase
    {
        private readonly AdminModel _model;
        public AdminsController(AdminModel model)
        {
            _model = model;
        }


        [Authorize]
        [HttpGet("me")]
        public ActionResult GetMe()
        {
            var name = User.Identity?.Name;
            var role = User.FindFirstValue(ClaimTypes.Role);
            return Ok(new { name, role });
        }

        [HttpPost("adminlogin")]
        public async Task<ActionResult> LogIn(
            [FromQuery] string username,
            [FromQuery] string password)
        {
            try
            {
                var admin = await _model.ValidateAdmin(username, password);
                if (admin is null)
                    return Unauthorized();

                List<Claim> claims = new()
                {
                    new Claim(ClaimTypes.NameIdentifier, admin.AdminId.ToString()),
                    new Claim(ClaimTypes.Name, admin.AdminName),
                    new Claim(ClaimTypes.Role, admin.Role)
                };

                var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(id);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return Ok(new { message = "Belepve", Role = admin.Role });
            }
            catch (ArgumentException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }


        [HttpPost("adminregistry")]
        public async Task<ActionResult> RegisterAdmin(
            [FromQuery] string username,
            [FromQuery] string password)
        {
            try
            {
                await _model.AdminRegistration(username, password);
                return Ok();
            }
            catch (ArgumentException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }


        [HttpPut("changepassword")]
        public async Task<ActionResult> ChangePassword(
            [FromQuery] int adminId,
            [FromQuery] string newPassword)
        {
            try
            {
                await _model.ChangePassword(adminId, newPassword);
                return Ok();
            }
            catch (ArgumentOutOfRangeException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (ArgumentException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("allusers")]
        public async Task<ActionResult<IEnumerable<UserDto>>> AllUsers()
        {
            try
            {
                var response = await _model.AllUsers();
                return Ok(response);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("allworkers")]
        public async Task<ActionResult<IEnumerable<WorkerDto>>> AllWorkers()
        {
            try
            {
                var response = await _model.AllWorkers();
                return Ok(response);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }


        [HttpPost("logout")]
        public async Task<ActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }
}