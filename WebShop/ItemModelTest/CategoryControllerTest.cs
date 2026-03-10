using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WebShop.Dto;

namespace ModelTest
{
    public class CategoryControllerTest : IClassFixture<CustomApplicationFactory>
    {
        private readonly CustomApplicationFactory _factory;
        private readonly HttpClient _client;

        public CategoryControllerTest(CustomApplicationFactory factory)
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

        //Seedelve: "Számítógépek", "Kiegészítők", "Alkatrészek"

        #region AllCategories
        [Fact]
        public async Task GetAllCategories_ReturnsOk()
        {
            var response = await _client.GetAsync("api/categories/allcategories");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAllCategories_ReturnsCategoryList()
        {
            var response = await _client.GetAsync("api/categories/allcategories");
            response.EnsureSuccessStatusCode();

            var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
            Assert.NotNull(categories);
            Assert.NotEmpty(categories);
            Assert.Contains(categories, c => c.categoryName == "Számítógépek");
        }
        #endregion

        #region AddNewCategory
        [Fact]
        public async Task AddNewCategory_ReturnsOk()
        {
            var workerClient = CreateWorkerClient();
            var response = await workerClient.PostAsync(
                "api/categories/addnewcategory?categ=ÚjKategóriaTeszt", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task AddNewCategory_EmptyName_BadRequest()
        {
            var workerClient = CreateWorkerClient();
            var response = await workerClient.PostAsync(
                "api/categories/addnewcategory?categ=", null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddNewCategory_Conflict()
        {
            var workerClient = CreateWorkerClient();
            var response = await workerClient.PostAsync(
                "api/categories/addnewcategory?categ=Számítógépek", null);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
        #endregion

        #region ModifyCategory
        [Fact]
        public async Task ModifyCategory_ReturnsOk()
        {
            var workerClient = CreateWorkerClient();
            var dto = new ModifyCategoryDto
            {
                categId = 2,
                categName = "KiegészítőkTeszt"
            };

            var response = await workerClient.PutAsJsonAsync(
                "api/categories/modifycategory", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ModifyCategory_InvalidId_BadRequest()
        {
            var workerClient = CreateWorkerClient();
            var dto = new ModifyCategoryDto
            {
                categId = -1,
                categName = "Valami"
            };

            var response = await workerClient.PutAsJsonAsync(
                "api/categories/modifycategory", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ModifyCategory_NotExistingId_NotFound()
        {
            var workerClient = CreateWorkerClient();
            var dto = new ModifyCategoryDto
            {
                categId = int.MaxValue,
                categName = "NemLetezo"
            };

            var response = await workerClient.PutAsJsonAsync(
                "api/categories/modifycategory", dto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ModifyCategory_EmptyName_BadRequest()
        {
            var workerClient = CreateWorkerClient();
            var dto = new ModifyCategoryDto
            {
                categId = 1,
                categName = ""
            };

            var response = await workerClient.PutAsJsonAsync(
                "api/categories/modifycategory", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ModifyCategory_Conflict()
        {
            var workerClient = CreateWorkerClient();
            var dto = new ModifyCategoryDto
            {
                categId = 3,
                categName = "Számítógépek"
            };

            var response = await workerClient.PutAsJsonAsync(
                "api/categories/modifycategory", dto);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
        #endregion

        #region DeleteCategory
        [Fact]
        public async Task DeleteCategory_ReturnsOk()
        {
            var workerClient = CreateWorkerClient();
            var createResponse = await workerClient.PostAsync(
                "api/categories/addnewcategory?categ=TorlendoKategoria", null);
            createResponse.EnsureSuccessStatusCode();

            var response = await workerClient.DeleteAsync(
                "api/categories/deletecategory?categid=4");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_InvalidId_BadRequest()
        {
            var workerClient = CreateWorkerClient();
            var response = await workerClient.DeleteAsync(
                "api/categories/deletecategory?categid=-1");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_NotFound()
        {
            var workerClient = CreateWorkerClient();
            var response = await workerClient.DeleteAsync(
                $"api/categories/deletecategory?categid={int.MaxValue}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion
    }
}
