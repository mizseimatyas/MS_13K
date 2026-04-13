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

        #region PlaceOrder
        [HttpPost("placeorder")]
        public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] PlaceOrderRequestDto dto)
        {
            try
            {
                var response = await _model.PlaceOrder(dto.userId, dto.targetAddress);
                return Ok(response);
            }
            catch (ArgumentOutOfRangeException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (ArgumentException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
        #endregion

        #region OrderHistory
        [HttpGet("orderhistory")]
        public async Task<ActionResult<List<OrderAllDto>>> OrderHistory([FromQuery] int userid)
        {
            try
            {
                var response = await _model.OrderHistoryByUserId(userid);
                return Ok(response);
            }
            catch (ArgumentOutOfRangeException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return BadRequest(ex.Message); }
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
            catch (ArgumentOutOfRangeException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return BadRequest(ex.Message); }
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
            catch (ArgumentOutOfRangeException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
        #endregion

        #region OrderDetailsByOrderId
        [HttpGet("orderDetailsByOrderId")]
        public async Task<ActionResult<OrderDetailsDto>> OrderDetailsByOrderId(
            [FromQuery] int orderId)
        {
            try
            {
                var response = await _model.OrderDetailsByOrderId(orderId);
                return Ok(response);
            }
            catch (ArgumentOutOfRangeException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
        #endregion

        #region AllOrders
        [HttpGet("allorders")]
        public async Task<ActionResult<List<OrderAllDto>>> AllOrders()
        {
            try
            {
                var response = await _model.GetAllOrders();
                return Ok(response);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
        #endregion

        #region UpdateOrderStatus
        [HttpPut("updateorderstatus")]
        public async Task<ActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusDto dto)
        {
            try
            {
                await _model.UpdateOrderStatus(dto);
                return Ok();
            }
            catch (ArgumentException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
        #endregion

        #region CompleteOrder
        [HttpPut("completeorder")]
        public async Task<ActionResult> CompleteOrder([FromQuery] int orderid)
        {
            try
            {
                await _model.CompleteOrder(orderid);
                return Ok();
            }
            catch (ArgumentOutOfRangeException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
        #endregion
    }
}
