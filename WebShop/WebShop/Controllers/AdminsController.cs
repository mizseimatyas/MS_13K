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

        [Authorize(Roles = "Admin")]
        [HttpPost("adminregistry")]
        public ActionResult RegisterAdmin([FromQuery]string username, [FromQuery] string password)
        {
            try
            {
                _model.AdminRegistration(username, password, "Admin");
                return Ok(); //change from void to async!!
            }
            catch(Exception)
            {
                return BadRequest();
            }
        }

        
        [HttpPost("adminlogin")]
        public async Task<ActionResult> LogIn(
            [FromQuery] string username,
            [FromQuery] string password)
        {
            try
            {
                var admin = _model.ValidateAdmin(username, password);
                if (admin == null)
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
            catch
            {
                return BadRequest();
            }
        }

        #region Change Password

        #endregion


        [HttpPost("/logout")]
        public async Task<ActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }
}
