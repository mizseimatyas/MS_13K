using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebShop.Dto;
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
    [FromQuery] string password,
    [FromQuery] int phone)
        {
            try
            {
                await _model.WorkerRegistration(username, password, phone);
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
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWorker(int id)
        {
            try
            {
                await _model.DeleteWorker(id);
                return Ok();
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

        [HttpPut("changedata/{id}")]
        public async Task<ActionResult> UpdateWorker(
            int id,
            [FromBody] ModifyWorkerDto dto)
        {
            try
            {
                await _model.ModifyWorkerData(id, dto.WorkerName, dto.Role, dto.Phone);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
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
        [HttpPost("logout")]
        public async Task<ActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }
}