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
    public class AdminsController : ControllerBase
    {
        private readonly AdminModel _model;
        public AdminsController(AdminModel model)
        {
            _model = model;
        }

        #region Admin Registration
        [Authorize(Roles = "Admin")]
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
            catch (ArgumentException)
            {
                return BadRequest();
            }
            catch (InvalidOperationException)
            {
                return Conflict(); // már létezik
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region Admin Login
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
            catch (ArgumentException)
            {
                return BadRequest();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region Change Password
        [Authorize(Roles = "Admin")]
        [HttpPost("changepassword")]
        public async Task<ActionResult> ChangePassword(
            [FromQuery] int adminId,
            [FromQuery] string newPassword)
        {
            try
            {
                await _model.ChangePassword(adminId, newPassword);
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
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        [HttpPost("logout")]
        public async Task<ActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }
}