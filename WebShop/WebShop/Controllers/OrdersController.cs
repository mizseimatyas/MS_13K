using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebShop.Dto;
using WebShop.Model;

namespace WebShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderModel _model;
        public OrdersController(OrderModel model)
        {
            _model = model;
        }

        #region OrderHistory
        [HttpGet("orderhistory")]
        public async Task<ActionResult<List<OrderAllDto>>> OrderHistory([FromQuery] int userid)
        {
            try
            {
                var response = await _model.OrderHistoryByUserId(userid);
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

        #region OrderDetails
        [HttpGet("orderdetails")]
        public async Task<ActionResult<OrderDetailsDto>> OrderDetails(
            [FromQuery] int userid,
            [FromQuery] int orderId)
        {
            try
            {
                var response = await _model.OrderDetailsByUserId(userid, orderId);
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

        #region CancelOrder
        [HttpPut("usercancelorder")]
        public async Task<ActionResult> CancelOrderByUser(
            [FromQuery] int orderid,
            [FromQuery] int userid)
        {
            try
            {
                await _model.CancelOrderByUserWithOrderId(orderid, userid);
                return Ok();
            }
            catch (ArgumentOutOfRangeException)
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

        #region AllOrders
        [Authorize(Roles = "Worker")]
        [HttpGet("allorders")]
        public async Task<ActionResult<List<OrderAllDto>>> AllOrders()
        {
            try
            {
                var response = await _model.GetAllOrders();
                return Ok(response);
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

        #region UpdateOrderStatus
        [Authorize(Roles = "Worker")]
        [HttpPut("updateorderstatus")]
        public async Task<ActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusDto dto)
        {
            try
            {
                await _model.UpdateOrderStatus(dto);
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

        #region CompleteOrder
        [Authorize(Roles = "Worker")]
        [HttpPut("completeorder")]
        public async Task<ActionResult> CompleteOrder([FromQuery] int orderid)
        {
            try
            {
                await _model.CompleteOrder(orderid);
                return Ok();
            }
            catch (ArgumentOutOfRangeException)
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
