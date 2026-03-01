using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WebShop.Dto;

namespace ModelTest
{
    public class OrderControllerTest : IClassFixture<CustomApplicationFactory>
    {
        private readonly HttpClient _client;

        public OrderControllerTest(CustomApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        //authorize, bejelentkezett user
        #region OrderHistory
        [Fact]
        public async Task OrderHistory_ReturnsOk()
        {
            var response = await _client.GetAsync("api/orders/orderhistory?userid=1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var orders = await response.Content.ReadFromJsonAsync<List<OrderAllDto>>();
            Assert.NotNull(orders);
            Assert.NotEmpty(orders);
        }

        [Fact]
        public async Task OrderHistory_InvalidUserId()
        {
            var response = await _client.GetAsync("api/orders/orderhistory?userid=0");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task OrderHistory_NotFound()
        {
            var response = await _client.GetAsync("api/orders/orderhistory?userid=9999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region OrderDetails
        [Fact]
        public async Task OrderDetails_ReturnsOk()
        {
            var response = await _client.GetAsync("api/orders/orderdetails?userid=1&orderId=1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var details = await response.Content.ReadFromJsonAsync<OrderDetailsDto>();
            Assert.NotNull(details);
            Assert.Equal(1, details.orderId);
        }

        [Fact]
        public async Task OrderDetails_InvalidId()
        {
            var response = await _client.GetAsync("api/orders/orderdetails?userid=0&orderId=0");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task OrderDetails_IdNotFound()
        {
            var response = await _client.GetAsync($"api/orders/orderdetails?userid=1&orderId={int.MaxValue}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region CancelOrder
        [Fact]
        public async Task CancelOrder_ReturnsOk()
        {
            var response = await _client.PutAsync("api/orders/usercancelorder?orderid=1&userid=1", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CancelOrder_InvalidId()
        {
            var response = await _client.PutAsync("api/orders/usercancelorder?orderid=0&userid=0", null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CancelOrder_NotFound()
        {
            var response = await _client.PutAsync(
                $"api/orders/usercancelorder?orderid={int.MaxValue}&userid=1", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion



        //authorize, bejelentkezett worker
        #region AllOrders

        #endregion

        #region UpdateOrderStatus

        #endregion

        #region CompleteOrder

        #endregion
    }
}
