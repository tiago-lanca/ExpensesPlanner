using ExpensesPlanner.Client.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace ExpensesPlanner.Client.Services
{
    public class ExpenseService
    {
        private readonly HttpClient _httpClient;
        public ExpenseService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        public async Task<List<Expense>> GetAllExpenses()
        {
            return await _httpClient.GetFromJsonAsync<List<Expense>>($"api/expenses") ?? new List<Expense>();
        }

        public async Task<List<Expense>> GetExpenseByNameAsync(string description)
        {
            return await _httpClient.GetFromJsonAsync<List<Expense>>($"api/Expenses/description/{description}") ?? new List<Expense>();
        }
    }
}
