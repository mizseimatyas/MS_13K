using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private readonly HttpClient _client;

        public ItemControllerTest(CustomApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

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
            var items = await response.Content.ReadFromJsonAsync<List<ItemDto>>();
            response.EnsureSuccessStatusCode();
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
            var item = await response.Content.ReadFromJsonAsync<ItemDto>();
            response.EnsureSuccessStatusCode();
            Assert.NotNull(item);
        }

        [Fact]
        public async Task GetItemById_ReturnsBadRequest()
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
            Assert.Equal(HttpStatusCode.NotFound,response.StatusCode);
        }

        #endregion

        #region ItemByNameSnipet
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
        public async Task GetItemsInPriceRange_ReturnsOutOfRange()
        {
            var response = await _client.GetAsync("api/items/itemsinpricerange?min=1&max=100");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        }
        #endregion




        #region AddNewItem (Auth)


        #endregion

        #region DeleteItem (Auth)

        #endregion

        #region ModifyItem (Auth)
        [Fact]
        public async Task ModifyItem_ReturnsOk()
        {
            
            var modifyDto = new ModifyItemDto
            {
                itemId = 1,
                itemName = "teszteles",
                categoryName = "Kiegészítők",
                quantity = 67,
                description = "kutyakaja",
                price = 200000
            };

            var response = await _client.PutAsJsonAsync("api/items/modifyitem", modifyDto);
            response.EnsureSuccessStatusCode();

        }


        #endregion




    }
}
