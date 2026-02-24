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
        public async Task<ActionResult> RegisterWorker(
            [FromQuery] string username,
            [FromQuery] string password)
        {
            try
            {
                await _model.WorkerRegistration(username, password);
                return Ok();
            }
            catch (InvalidOperationException)
            {
                return Conflict();
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

        [HttpPost("workerlogin")]
        public async Task<ActionResult> LogIn(
            [FromQuery] string username,
            [FromQuery] string password)
        {
            try
            {
                var worker = await _model.ValidateWorker(username, password);
                if (worker is null)
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
            catch (ArgumentException)
            {
                return BadRequest();
            }
            catch
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "Worker,Admin")]
        [HttpPut("changepassword")]
        public async Task<ActionResult> ChangePassword(
            [FromQuery] int workerId,
            [FromQuery] string newPassword)
        {
            try
            {
                await _model.ChangePassword(workerId, newPassword);
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

        [HttpPost("logout")]
        public async Task<ActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }
}