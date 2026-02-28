using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ModelTest
{
    public class UserControllerTest : IClassFixture<CustomApplicationFactory>
    {
        private readonly HttpClient _client;

        public UserControllerTest(CustomApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        #region Registration
        [Fact]
        public async Task Registration_ReturnsOk()
        {
            var email = "newuser@gmail.com";
            var password = "jelszo123";

            var response = await _client.PostAsync($"api/users/userregistry?email={email}&password={password}", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Registration_ReturnsBadRequest()
        {
            var email = "nemjo";
            var password = "";

            var response = await _client.PostAsync($"api/users/userregistry?email={email}&password={password}", null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region Login
        [Fact]
        public async Task Login_ReturnsOk()
        {
            var response = await _client.PostAsync(
                "api/users/loginuser?email=gamer@example.com&password=pass123", null);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.NotNull(result);
            Assert.Equal("User", result.Role);
            Assert.Equal("Belepve", result.message);
        }

        [Fact]
        public async Task LoginWrongPassword_ReturnsBadRequest()
        {
            var response = await _client.PostAsync(
                "api/users/loginuser?email=gamer@example.com&password=rosszjelszo", null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task LoginEmpty_ReturnsBadRequest()
        {
            var response = await _client.PostAsync("api/users/loginuser?email=&password=", null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region ChangePassword
        //update ötlet: var username, var jelszo,loginresponsedto-ba id-t rakni, az az id megy tovább jelszováltásnál
        [Fact]
        public async Task ChangePassword_ReturnsOk()
        {
            var loginResponse = await _client.PostAsync(
                "api/users/loginuser?email=gamer@example.com&password=pass123", null);
            loginResponse.EnsureSuccessStatusCode();

            var response = await _client.PutAsync("api/users/changepassword?userid=1&newpassword=NewPass123", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /* Kell vagy nemkell, authorize miatt login fix kell, utána nem tudja elhagyni az idt
        [Fact]
        public async Task ChangePassword_Invalid_ReturnsBadRequest()
        {
            var response = await _client.PutAsync(
                "api/users/changepassword?userid=-1&newpassword=Valami123!", null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_UserNotFound_ReturnsBadRequest()
        {
            var response = await _client.PutAsync(
                "api/users/changepassword?userid=9999&newpassword=Valami123!", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }*/

        #endregion

        #region Logout
        [Fact]
        public async Task Logout_ReturnsOk()
        {
            var loginResponse = await _client.PostAsync("api/users/loginuser?email=tesztfelhasznalo@example.com&password=pass123", null);
            loginResponse.EnsureSuccessStatusCode();

            var logoutResponse = await _client.PostAsync("api/users/logout", null);
            Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
        }

        [Fact]
        public async Task UserLogout_WithoutLogin_Unauthorized()
        {
            var response = await _client.PostAsync("api/users/logout", null);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        #endregion

    }
    public class LoginResponseDto
    {
        public string message { get; set; }
        public string Role { get; set; }
    }
}