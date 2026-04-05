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
    public class OrdersModel
    {
        private readonly HttpClient _client;

        public OrdersModel(string baseUrl)
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public async Task<List<OrderAllDto>?> GetAllOrdersAsync()
        {
            var response = await _client.GetAsync("api/orders/allorders");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<List<OrderAllDto>>();
        }

        public async Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusDto dto)
        {
            var response = await _client.PutAsJsonAsync("api/Orders/updateorderstatus", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<OrderDetailsDto?> GetOrderDetailsAsync(int orderId)
        {
            var response = await _client.GetAsync($"api/Orders/orderDetailsByOrderId?orderId={orderId}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<OrderDetailsDto>();
        }
    }
}
