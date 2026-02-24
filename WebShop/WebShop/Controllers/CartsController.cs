using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebShop.Dto;
using WebShop.Model;

namespace WebShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly CartModel _model;
        public CartsController(CartModel model)
        {
            _model = model;
        }

        #region CartInventoryByUserId
        [HttpGet("cartinventory")]
        public async Task<ActionResult<CartDto>> CartInventory([FromQuery] int userid)
        {
            try
            {
                var response = await _model.CartInventoryByUserId(userid);
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
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region CartInventoryTotalPrice
        [HttpGet("cartinventorytotalprice")]
        public async Task<ActionResult<int>> CartTotalPrice([FromQuery] int userid)
        {
            try
            {
                var response = await _model.CartInventoryTotalPrice(userid);
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
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region ModifyCartItems
        [HttpPut("modifycart")]
        public async Task<ActionResult> ModifyCartItems([FromBody] ModifyCartItemDto dto)
        {
            try
            {
                await _model.ModifyCartItems(dto);
                return Ok();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException)
            {
                return Conflict();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion
    }
}
