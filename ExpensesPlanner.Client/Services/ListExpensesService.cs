using ExpensesPlanner.Client.Models;
using System.Net.Http.Json;

namespace ExpensesPlanner.Client.Services
{
    public class ListExpensesService
    {
        private readonly HttpClient _httpClient;
        public ListExpensesService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
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

        public async Task<ListExpenses> GetListByUserIdAsync(string id)
        {
            return await _httpClient.GetFromJsonAsync<ListExpenses>($"api/listexpenses/user/{id}") ?? new ListExpenses();
        }

        public async Task<HttpResponseMessage> UpdateListExpensesAsync(ListExpenses list)
        {
            return await _httpClient.PutAsJsonAsync($"api/listexpenses/{list.Id}", list);
        }
    }
}
