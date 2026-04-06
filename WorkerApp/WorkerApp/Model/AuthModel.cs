using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WorkerApp.Dto;

namespace WorkerApp.Model
{
    public class AuthModel
    {
        private readonly HttpClient _client;

        public AuthModel(HttpClient client)
        {
            _client = client;
        }

        public async Task<LoginResponseDto?> LoginAsync(string username, string password, string roleKey)
        {
            var endpoint = roleKey == "admin"
                ? "api/Admins/adminlogin"
                : "api/Workers/workerlogin";

            var url = $"{endpoint}?username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}";

            var response = await _client.PostAsync(url, null);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        }
    }
}

