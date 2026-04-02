using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Numerics;
using System.Security.Claims;
using WebShop.Dto;
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

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetMe()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userIdClaim))
                    return Unauthorized();

                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized();

                var response = await _model.GetMe(userId);
                return Ok(response);
            }
            catch (ArgumentOutOfRangeException)
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

        [HttpPost("userregistry")]
        public async Task<ActionResult> Registration(
            [FromQuery] string email,
            [FromQuery] string password,
            [FromQuery] string? address,
            [FromQuery] string? phone)
        {
            try
            {
                await _model.Registration(email, password, address, phone);
                return Ok();
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
        [HttpPut("updateprofile")]
        public async Task<ActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(userIdClaim))
                    return Unauthorized("Hiányzó felhasználói azonosító.");

                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized("Érvénytelen felhasználói azonosító.");

                await _model.UpdateProfile(
                    userId,
                    dto.email,
                    dto.name,
                    dto.city,
                    dto.zipCode,
                    dto.address,
                    dto.phone
                );

                return Ok();
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception)
            {
                return BadRequest("Váratlan hiba történt.");
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