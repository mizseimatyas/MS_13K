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
    public class WorkersController : ControllerBase
    {
        private readonly WorkerModel _model;
        public WorkersController(WorkerModel model)
        {
            _model = model;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("workerregistry")]
        public ActionResult RegisterAdmin([FromQuery] string username, [FromQuery] string password)
        {
            try
            {
                _model.WorkerRegistration(username, password, "Worker");
                return Ok(); //change from void to async!!
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost("workerlogin")]
        public async Task<ActionResult> LogIn(
            [FromQuery] string username,
            [FromQuery] string password)
        {
            try
            {
                var worker = _model.ValidateWorker(username, password);
                if (worker == null)
                    return Unauthorized();

                List<Claim> claims = new()
                {
                    new Claim(ClaimTypes.NameIdentifier, worker.WorkerId.ToString()),
                    new Claim(ClaimTypes.Name, worker.WorkerName),
                    new Claim(ClaimTypes.Role, worker.Role)
                };

                var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(id);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return Ok(new { message = "Belepve", Role = worker.Role });
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
