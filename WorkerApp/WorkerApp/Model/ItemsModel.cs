using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WorkerApp.Dto;

namespace WorkerApp.Model
{
    public class ItemsModel
    {
        private readonly HttpClient _client;

        public ItemsModel(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<ItemDto>?> GetAllItemsAsync()
        {
            var response = await _client.GetAsync("api/Items/allitems");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<List<ItemDto>>();
        }

        public async Task<bool> UpdateItemAsync(ItemDto item)
        {
            var dto = new ModifyItemDto
            {
                ItemId = item.ItemId,
                CategoryName = item.CategoryName,
                ItemName = item.ItemName,
                Quantity = item.Quantity,
                Description = item.Description,
                Price = item.Price
            };

            var response = await _client.PutAsJsonAsync("api/Items/modifyitem", dto);
            return response.IsSuccessStatusCode;
        }
    }
}