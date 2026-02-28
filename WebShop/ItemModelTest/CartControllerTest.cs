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
    public class CartControllerTest : IClassFixture<CustomApplicationFactory>
    {
        private readonly HttpClient _client;

        public CartControllerTest(CustomApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        #region CartInventoryByUserId
        [Fact]
        public async Task CartInventory_ReturnsOk()
        {
            var response = await _client.GetAsync("api/carts/cartinventory?userid=1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var cart = await response.Content.ReadFromJsonAsync<CartDto>();
            Assert.NotNull(cart);
            Assert.Equal(1, cart!.userId);
            Assert.NotNull(cart.itemList);
            Assert.NotEmpty(cart.itemList);
        }

        [Fact]
        public async Task CartInventory_IdNotFound()
        {
            var response = await _client.GetAsync("api/carts/cartinvenotry?userid=9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region CartInventoryTotalPrice
        [Fact]
        public async Task CartInventoryTotalPrice_ReturnsOk()
        {
            var response = await _client.GetAsync("api/carts/cartinventorytotalprice?userid=1");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var total = await response.Content.ReadFromJsonAsync<int>();
            Assert.True(total > 0);
        }

        [Fact]
        public async Task CartInventoryTotalPrice_InvalidUserId()
        {
            var response = await _client.GetAsync("api/carts/cartinventorytotalprice?userid=0");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CartInventoryTotalPrice_IdNotFound()
        {
            var response = await _client.GetAsync("api/carts/cartinventorytotalprice?userid=9999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region ModifyCartItems
        [Fact]
        public async Task ModifyCartItems_ReturnsOk()
        {
            var dto = new ModifyCartItemDto
            {
                userId = 1,
                itemId = 1,
                quantity = 2,
                price = 0
            };
            var response = await _client.PutAsJsonAsync("api/carts/modifycart", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ModifyCartItems_InvalidUserId()
        {
            var dto = new ModifyCartItemDto
            {
                userId = 0,
                itemId = 1,
                quantity = 1,
                price = 0
            };

            var response = await _client.PutAsJsonAsync("api/carts/modifycart", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ModifyCartItems_InvalidItemId()
        {
            var dto = new ModifyCartItemDto
            {
                userId = 1,
                itemId = 0,
                quantity = 1,
                price = 0
            };

            var response = await _client.PutAsJsonAsync("api/carts/modifycart", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ModifyCartItems_NegativeQuantity()
        {
            var dto = new ModifyCartItemDto
            {
                userId = 1,
                itemId = 1,
                quantity = -1,
                price = 0
            };

            var response = await _client.PutAsJsonAsync("api/carts/modifycart", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        //update(kész): raktar mennyisegnel nagyobb
        [Fact]
        public async Task ModifyCartItems_QuantityTooHigh()
        {
            var cartResponse = await _client.GetAsync("api/carts/cartinventory?userid=1");
            cartResponse.EnsureSuccessStatusCode();

            var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();
            var anyItem = cart!.itemList.First();

            var dto = new ModifyCartItemDto
            {
                userId = cart.userId,
                itemId = anyItem.itemId,
                quantity = 9999,
                price = 0
            };

            var response = await _client.PutAsJsonAsync("api/carts/modifycart", dto);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task ModifyCartItems_QuantityZero_RemovesItemOk()
        {
            var dto1 = new ModifyCartItemDto
            {
                userId = 1,
                itemId = 1,
                quantity = 1,
                price = 0
            };

            var initResponse = await _client.PutAsJsonAsync("api/carts/modifycart", dto1);
            initResponse.EnsureSuccessStatusCode();

            var dto2 = new ModifyCartItemDto
            {
                userId = 1,
                itemId = 1,
                quantity = 0,
                price = 0
            };

            var response = await _client.PutAsJsonAsync("api/carts/modifycart", dto2);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        #endregion
    }
}
