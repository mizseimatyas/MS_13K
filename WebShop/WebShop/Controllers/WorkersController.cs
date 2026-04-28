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
    public class WorkersController : ControllerBase
    {
        private readonly WorkerModel _model;
        public WorkersController(WorkerModel model)
        {
            _model = model;
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
            catch (ArgumentException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }


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
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
            catch (ArgumentException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWorker([FromQuery]int id)
        {
            try
            {
                await _model.DeleteWorker(id);
                return Ok();
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("changedata/{id}")]
        public async Task<ActionResult> UpdateWorker(
            [FromQuery]int id,
            [FromBody] ModifyWorkerDto dto)
        {
            try
            {
                await _model.ModifyWorkerData(id, dto.WorkerName, dto.Role, dto.Phone);
                return Ok();
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (ArgumentException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

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
            catch (ArgumentOutOfRangeException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (ArgumentException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
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