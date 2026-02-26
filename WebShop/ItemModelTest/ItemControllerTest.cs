using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ModelTest
{
    public class ItemControllerTest : IClassFixture<CustomApplicationFactory>
    {
        private readonly HttpClient _client;

        public ItemControllerTest(CustomApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetItems_ReturnsOk()
        {
            var response = await _client.GetAsync("api/items/allitems");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

    }
}
