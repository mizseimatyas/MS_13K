using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WebShop.Persistence;
using WebShop.Utils;

namespace ModelTest
{
    public class AdminControllerTest : IClassFixture<CustomApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomApplicationFactory _factory;
        public AdminControllerTest(CustomApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Seed_Diagnostic()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();

            var users = db.Users.ToList();
            var admins = db.Admins.ToList();
            var workers = db.Workers.ToList();

            Assert.NotEmpty(users);
            Assert.NotEmpty(admins);
            Assert.NotEmpty(workers);

            var admin = admins.First();
            var worker = workers.First();

            // Név és szerepkör ellenőrzés
            Assert.Equal("WebshopAdmin", admin.AdminName);
            Assert.Equal("Admin", admin.Role);

            Assert.Equal("RaktarosPC", worker.WorkerName);
            Assert.Equal("Worker", worker.Role);

            // Hash ellenőrzés – itt látjuk, hogy a seeder és a PasswordHasher egyezik-e
            var adminHashExpected = PasswordHasher.Hash("admin123");
            var workerHashExpected = PasswordHasher.Hash("worker123");

            Assert.Equal(adminHashExpected, admin.Password);
            Assert.Equal(workerHashExpected, worker.Password);
        }



        //authorize miatt hamarabb kell a bejelentkezés mint a regisztráció
        // AdminName = "WebshopAdmin"
        // Password = "admin123" -> hash
        #region Login 
        [Fact]
        public async Task AdminLogin_ReturnsOk()
        {
            var response = await _client.PostAsync(
                "api/admins/adminlogin?username=WebshopAdmin&password=admin123", null);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<AdminLoginResponseDto>();
            Assert.NotNull(result);
            Assert.Equal("Admin", result.Role);
            Assert.Equal("Belepve", result.message);
        }

        [Fact]
        public async Task AdminLogin_WrongPassword_Unauthorized()
        {
            var response = await _client.PostAsync(
                "api/admins/adminlogin?username=WebshopAdmin&password=rossz", null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AdminLogin_Empty_BadRequest()
        {
            var response = await _client.PostAsync(
                "api/admins/adminlogin?username=&password=", null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Register
        [Fact]
        public async Task RegisterAdmin_ReturnsOk()
        {
            var loginResponse = await _client.PostAsync(
                "api/admins/adminlogin?username=WebshopAdmin&password=admin123", null);
            loginResponse.EnsureSuccessStatusCode();

            var response = await _client.PostAsync(
                "api/admins/adminregistry?username=testadmin123&password=NewAdmin123!", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task RegisterAdmin_Unauthorized()
        {
            var response = await _client.PostAsync(
                "api/admins/adminregistry?username=valaki&password=Valami123!", null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        #endregion

        #region ChangePassword
        [Fact]
        public async Task ChangePassword_ReturnsOk()
        {
            // Admin login
            var loginResponse = await _client.PostAsync(
                "api/admins/adminlogin?username=WebshopAdmin&password=admin123", null);
            loginResponse.EnsureSuccessStatusCode();

            // AdminId lekérése DB-ből név alapján
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();
            var admin = db.Admins.Single(a => a.AdminName == "WebshopAdmin");
            var adminId = admin.AdminId;

            var response = await _client.PostAsync(
                $"api/admins/changepassword?adminId={adminId}&newPassword=NewAdmin456", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_InvalidId_Badrequest()
        {
            var loginResponse = await _client.PostAsync(
                "api/admins/adminlogin?username=WebshopAdmin&password=admin123", null);
            loginResponse.EnsureSuccessStatusCode();

            var response = await _client.PostAsync(
                "api/admins/changepassword?adminId=-1&newPassword=asd", null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_AdminNotFound()
        {
            var loginResponse = await _client.PostAsync(
                "api/admins/adminlogin?username=WebshopAdmin&password=admin123", null);
            loginResponse.EnsureSuccessStatusCode();

            // biztosan nem létező id (int.MaxValue)
            var response = await _client.PostAsync(
                $"api/admins/changepassword?adminId={int.MaxValue}&newPassword=Valami123", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region Logout
        [Fact]
        public async Task AdminLogout_ReturnsOk()
        {
            var loginResponse = await _client.PostAsync(
                "api/admins/adminlogin?username=WebshopAdmin&password=admin123", null);
            loginResponse.EnsureSuccessStatusCode();

            var logoutResponse = await _client.PostAsync(
                "api/admins/logout", null);

            Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
        }
        #endregion
    }

    public class AdminLoginResponseDto
    {
        public string message { get; set; }
        public string Role { get; set; }
    }
}