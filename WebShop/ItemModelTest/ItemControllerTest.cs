using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WebShop.Controllers;
using WebShop.Dto;
using WebShop.Persistence;

namespace ModelTest
{
    public class ItemControllerTest : IClassFixture<CustomApplicationFactory>
    {
        private readonly CustomApplicationFactory _factory;
        private readonly HttpClient _client;

        public ItemControllerTest(CustomApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region Helper
        private HttpClient CreateWorkerClient()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Test");
            return client;
        }
        #endregion

        #region AllItems
        [Fact]
        public async Task GetItems_ReturnsOk()
        {
            var response = await _client.GetAsync("api/items/allitems");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetItems_ReturnsItemList()
        {
            var response = await _client.GetAsync("api/items/allitems");
            response.EnsureSuccessStatusCode();
            var items = await response.Content.ReadFromJsonAsync<List<ItemDto>>();
            Assert.NotNull(items);
            Assert.NotEmpty(items);
        }
        #endregion

        #region ItemById
        [Fact]
        public async Task GetItemById_ReturnsOk()
        {
            var response = await _client.GetAsync("api/items/itembyid?id=2");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetItemById_ReturnsItemDto()
        {
            var response = await _client.GetAsync("api/items/itembyid?id=2");
            response.EnsureSuccessStatusCode();
            var item = await response.Content.ReadFromJsonAsync<ItemDto>();
            Assert.NotNull(item);
        }

        [Fact]
        public async Task GetItemById_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/items/itembyid?id=-1");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region ItemByName
        [Fact]
        public async Task GetItemByName_ReturnsOk()
        {
            var response = await _client.GetAsync("api/items/itembyname?name=Desktop PC Ryzen7 RTX3060");
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GetItemByName_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/items/itembyname?name=nemjo");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region ItemByNameSnippet
        [Fact]
        public async Task GetItemByNameFragment_ReturnsOk()
        {
            var response = await _client.GetAsync("api/items/itemnamebyfragment?fragname=Desktop");
            response.EnsureSuccessStatusCode();
            var items = await response.Content.ReadFromJsonAsync<List<SearchItemsByDto>>();
            Assert.NotNull(items);
        }
        #endregion

        #region ItemsInCategory
        [Fact]
        public async Task GetItemsInCategory_ReturnsOk()
        {
            var response = await _client.GetAsync("api/items/itemsincategory?category=Számítógépek");
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GetItemsInCategory_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/items/itemsincategory?category=nemjo");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region ItemsByPriceRange
        [Fact]
        public async Task GetItemsInPriceRange_ReturnsOk()
        {
            var response = await _client.GetAsync("api/items/itemsinpricerange?min=10000&max=60000");
            response.EnsureSuccessStatusCode();
            var items = await response.Content.ReadFromJsonAsync<List<SearchItemsByPriceDto>>();
            Assert.NotNull(items);
        }

        [Fact]
        public async Task GetItemsInPriceRange_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/items/itemsinpricerange?min=1&max=100");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region ItemsInCategoryPriceDesc
        [Fact]
        public async Task ItemsInCategoryPriceDesc_Correct()
        {
            var response = await _client.GetAsync("api/items/itemsincategorypricedesc?category=Számítógépek");
            response.EnsureSuccessStatusCode();
            var items = await response.Content.ReadFromJsonAsync<List<SearchItemsByDto>>();
            Assert.NotNull(items);
            for (int i = 0; i < items.Count - 1; i++)
            {
                Assert.True(items[i].pricE >= items[i + 1].pricE);
            }
        }

        [Fact]
        public async Task ItemsInCategoryPriceDesc_NotFound()
        {
            var response = await _client.GetAsync("api/items/itemsincategorypricedesc?category=Nemjó");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region AddNewItem (Auth)
        [Fact]
        public async Task AddNewItem_ReturnsOk()
        {
            var workerClient = CreateWorkerClient();
            var dto = new AddNewItemDto
            {
                categoryName = "Számítógépek",
                itemName = "Kaja",
                quantity = 67,
                description = "Rizz",
                price = 69000,
            };

            var response = await workerClient.PostAsJsonAsync("api/items/addnewitem", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task AddNewItem_ThrowsNotFoundCategory()
        {
            var workerClient = CreateWorkerClient();
            var dto = new AddNewItemDto
            {
                categoryName = "Kutyakaja",
                itemName = "Kaja",
                quantity = 67,
                description = "Rizz",
                price = 69000,
            };

            var response = await workerClient.PostAsJsonAsync("api/items/addnewitem", dto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddNewItem_ThrowsPriceNegative()
        {
            var workerClient = CreateWorkerClient();
            var dto = new AddNewItemDto
            {
                categoryName = "Kutyakaja",
                itemName = "Kaja",
                quantity = 67,
                description = "Rizz",
                price = -69000,
            };

            var response = await workerClient.PostAsJsonAsync("api/items/addnewitem", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region DeleteItem (Auth)
        [Fact]
        public async Task DeleteItem_ReturnsOk()
        {
            var workerClient = CreateWorkerClient();

            var response = await workerClient.PostAsync("api/items/deleteitem?id=1", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DeleteItem_IdNotFound()
        {
            var workerClient = CreateWorkerClient();

            var response = await workerClient.PostAsync("api/items/deleteitem?id=1000", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteItem_Invalid()
        {
            var workerClient = CreateWorkerClient();

            var response = await workerClient.PostAsync("api/items/deleteitem?id=-10", null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region ModifyItem (Auth)
        [Fact]
        public async Task ModifyItem_ReturnsOk()
        {
            var workerClient = CreateWorkerClient();
            var modifyDto = new ModifyItemDto
            {
                itemId = 1,
                itemName = "teszteles",
                categoryName = "Kiegészítők",
                quantity = 67,
                description = "kutyakaja",
                price = 200000
            };

            var response = await workerClient.PostAsJsonAsync("api/items/modifyitem", modifyDto);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task ModifyItem_ThrowsNoName()
        {
            var workerClient = CreateWorkerClient();
            var modifyDto = new ModifyItemDto
            {
                itemId = 1,
                itemName = "",
                categoryName = "Kiegészítők",
                quantity = 67,
                description = "kutyakaja",
                price = 200000
            };

            var response = await workerClient.PostAsJsonAsync("api/items/modifyitem", modifyDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ModifyItem_Invalid()
        {
            var workerClient = CreateWorkerClient();
            var modifyDto = new ModifyItemDto
            {
                itemId = 5000,
                itemName = "Vau",
                categoryName = "Kiegészítők",
                quantity = 67,
                description = "kutyakaja",
                price = 200000
            };

            var response = await workerClient.PostAsJsonAsync("api/items/modifyitem", modifyDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion
    }
}