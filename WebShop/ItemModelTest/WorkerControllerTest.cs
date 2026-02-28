using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WebShop.Persistence;

namespace ModelTest
{
    public class WorkerControllertest : IClassFixture<CustomApplicationFactory>
    {
        private readonly HttpClient _client;

        public WorkerControllertest(CustomApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        // csak Admin regisztrálhat Workert -> Admin login
        // WorkerName = "RaktarosPC"
        // Password = "worker123" -> hash
        #region Login
        [Fact]
        public async Task WorkerLogin_ReturnsOk()
        {
            var response = await _client.PostAsync(
                "api/workers/workerlogin?username=RaktarosPC&password=worker123", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task WorkerLogin_WrongPassword_Unauthorized()
        {
            var response = await _client.PostAsync(
                "api/workers/workerlogin?username=RaktarosPC&password=rossz", null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task WorkerLogin_Empty_BadRequest()
        {
            var response = await _client.PostAsync(
                "api/workers/workerlogin?username=&password=", null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Register
        [Fact]
        public async Task RegisterWorker_ReturnsOk()
        {
            //Admin login
            var loginResponse = await _client.PostAsync(
                "api/admins/adminlogin?username=WebshopAdmin&password=admin123", null);
            loginResponse.EnsureSuccessStatusCode();

            var response = await _client.PostAsync(
                "api/workers/workerregistry?username=UjRaktaros&password=WorkerPass123!", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task RegisterWorker_Unauthorized()
        {
            var response = await _client.PostAsync(
                "api/workers/workerregistry?username=Valaki&password=Valami123!", null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        #endregion

        #region ChangePassword
        [Fact]
        public async Task ChangePassword_ReturnsOk()
        {
            var loginResponse = await _client.PostAsync(
                "api/workers/workerlogin?username=RaktarosPC&password=worker123", null);
            loginResponse.EnsureSuccessStatusCode();

            var response = await _client.PutAsync(
                "api/workers/changepassword?workerId=1&newPassword=NewWorker456", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_Admin_ReturnsOk()
        {
            var loginResponse = await _client.PostAsync(
                "api/admins/adminlogin?username=WebshopAdmin&password=admin123", null);
            loginResponse.EnsureSuccessStatusCode();

            var response = await _client.PutAsync(
                "api/workers/changepassword?workerId=1&newPassword=AdminSetsNewPass", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_InvalidId_BadRequest()
        {
            var loginResponse = await _client.PostAsync(
                "api/workers/workerlogin?username=RaktarosPC&password=worker123", null);
            loginResponse.EnsureSuccessStatusCode();

            var response = await _client.PutAsync(
                "api/workers/changepassword?workerId=-1&newPassword=asd", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_WorkerNotFound()
        {
            var loginResponse = await _client.PostAsync(
                "api/workers/workerlogin?username=RaktarosPC&password=worker123", null);
            loginResponse.EnsureSuccessStatusCode();

            var response = await _client.PutAsync(
                "api/workers/changepassword?workerId=9999&newPassword=Valami123", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_Unauthorized()
        {
            var response = await _client.PutAsync(
                "api/workers/changepassword?workerId=1&newPassword=Valami123", null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        #endregion

        #region Logout
        [Fact]
        public async Task WorkerLogout_ReturnsOk()
        {
            var loginResponse = await _client.PostAsync(
                "api/workers/workerlogin?username=RaktarosPC&password=worker123", null);
            loginResponse.EnsureSuccessStatusCode();

            var logoutResponse = await _client.PostAsync(
                "api/workers/logout", null);

            Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
        }
        #endregion

    }
    public class WorkerLoginResponseDto
    {
        public string message { get; set; }
        public string Role { get; set; }
    }
}