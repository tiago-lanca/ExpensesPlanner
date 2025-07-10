using ExpensesPlanner.Client.Interfaces;
using ExpensesPlanner.Client.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace ExpensesPlanner.Client.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly HttpClient _httpClient;
        public ExpenseService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Expense> CreateExpenseAsync(Expense expense)
        {
            var response = await _httpClient.PostAsJsonAsync("api/expenses", expense);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Expense>() ?? new Expense();
        }        

        public async Task<List<Expense>> GetAllExpenses()
        {
            return await _httpClient.GetFromJsonAsync<List<Expense>>($"api/expenses") ?? new List<Expense>();
        }

        public async Task<Expense> GetExpenseByIdAsync(string id)
        {
            return await _httpClient.GetFromJsonAsync<Expense>($"api/expenses/{id}") ?? new Expense();
        }

        public async Task<List<Expense>> GetExpenseByNameAsync(string description)
        {
            return await _httpClient.GetFromJsonAsync<List<Expense>>($"api/Expenses/description/{description}") ?? new List<Expense>();
        }

        public Task<HttpResponseMessage> UpdateExpenseAsync(Expense expense)
        {
            return _httpClient.PutAsJsonAsync($"api/expenses/{expense.Id}", expense);
        }

        public async Task<HttpResponseMessage> DeleteExpenseAsync(string id)
        {
            return await _httpClient.DeleteAsync($"api/expenses/{id}");
        }
    }
}
