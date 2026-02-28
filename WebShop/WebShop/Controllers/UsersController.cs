using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebShop.Model;

namespace WebShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserModel _model;
        public UsersController(UserModel model)
        {
            _model = model;
        }

        [HttpPost("userregistry")]
        public async Task<ActionResult> Registration(
            [FromQuery] string email,
            [FromQuery] string password)
        {
            try
            {
                await _model.Registration(email, password);
                return Ok();
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost("loginuser")]
        public async Task<ActionResult> LogIn(
            [FromQuery] string email,
            [FromQuery] string password)
        {
            try
            {
                var user = await _model.ValidateUser(email, password);
                if (user is null)
                    return Unauthorized();

                List<Claim> claims = new()
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(id);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return Ok(new { message = "Belepve", Role = user.Role });
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
            catch
            {
                return BadRequest();
            }
        }

        [Authorize]
        [HttpPut("changepassword")]
        public async Task<ActionResult> ChangePassword(
            [FromQuery] int userid,
            [FromQuery] string newpassword)
        {
            try
            {
                await _model.ChangePassword(userid, newpassword);
                return Ok();
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch
            {
                return BadRequest();
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }
}