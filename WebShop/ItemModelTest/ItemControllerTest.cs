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
            var dto = new AddNewItemDto
            {
                categoryName = "Számítógépek",
                itemName = "Kaja",
                quantity = 67,
                description = "Rizz",
                price = 69000,
            };
            var response = await _client.PutAsJsonAsync("api/items/addnewitem", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task AddNewItem_ThrowsNotFoundCategory()
        {
            var dto = new AddNewItemDto
            {
                categoryName = "Kutyakaja",
                itemName = "Kaja",
                quantity = 67,
                description = "Rizz",
                price = 69000,
            };
            var response = await _client.PutAsJsonAsync("api/items/addnewitem", dto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddNewItem_ThrowsPriceNegative()
        {
            var dto = new AddNewItemDto
            {
                categoryName = "Kutyakaja",
                itemName = "Kaja",
                quantity = 67,
                description = "Rizz",
                price = -69000,
            };
            var response = await _client.PutAsJsonAsync("api/items/addnewitem", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region DeleteItem (Auth)
        [Fact]
        public async Task DeleteCategory_ReturnsOk()
        {
            var response = await _client.DeleteAsync(
                "api/items/deleteitem?id=1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_IdNotFound()
        {
            var response = await _client.DeleteAsync(
                "api/items/deleteitem?id=1000");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_Invalid()
        {
            var response = await _client.DeleteAsync(
                "api/items/deleteitem?id=-10");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
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

        [Fact]
        public async Task ModifyItem_ThrowsNoName()
        {

            var modifyDto = new ModifyItemDto
            {
                itemId = 1,
                itemName = "",
                categoryName = "Kiegészítők",
                quantity = 67,
                description = "kutyakaja",
                price = 200000
            };

            var response = await _client.PutAsJsonAsync("api/items/modifyitem", modifyDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        }

        [Fact]
        public async Task ModifyItem_Invalid()
        {

            var modifyDto = new ModifyItemDto
            {
                itemId = 5000,
                itemName = "Vau",
                categoryName = "Kiegészítők",
                quantity = 67,
                description = "kutyakaja",
                price = 200000
            };

            var response = await _client.PutAsJsonAsync("api/items/modifyitem", modifyDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        }
        #endregion
    }
}
