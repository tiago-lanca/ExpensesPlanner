using ExpensesPlanner.Client.Models;
using System.Net.Http.Json;

namespace ExpensesPlanner.Client.Services
{
    public class ListExpensesService
    {
        private readonly HttpClient _httpClient;
        public ListExpensesService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ListExpenses> CreateListExpensesAsync(ListExpenses listExpenses)
        {
            var response = await _httpClient.PostAsJsonAsync("api/listexpenses", listExpenses);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ListExpenses>() ?? new ListExpenses();
        }

        public async Task<ListExpenses> GetListByIdAsync(string id)
        {
            return await _httpClient.GetFromJsonAsync<ListExpenses>($"api/listexpenses/{id}") ?? new ListExpenses();
        }

        public async Task<ListExpenses> GetListByUserIdAsync(ApplicationUser user)
        {
            _httpClient.DefaultRequestHeaders.Remove(ApiKeyService.API_KEY_HEADER);
            _httpClient.DefaultRequestHeaders.Add(ApiKeyService.API_KEY_HEADER, user.ApiKeyHash);

            return await _httpClient.GetFromJsonAsync<ListExpenses>($"api/listexpenses/user/{user.Id}") ?? new ListExpenses();
        }

        public async Task<HttpResponseMessage> UpdateListExpensesAsync(ListExpenses list)
        {
            return await _httpClient.PutAsJsonAsync($"api/listexpenses/{list.Id}", list);
        }
    }
}
