using Blazored.LocalStorage;
using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Interfaces;
using ExpensesPlanner.Client.Models;
using System.Net.Http.Json;

namespace ExpensesPlanner.Client.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public UserService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task<List<UserDetails>> GetAllUsers()
        {
            string? apiKey = await _localStorage.GetItemAsync<string>("apiKey");
            _httpClient.DefaultRequestHeaders.Add(ApiKeyService.API_KEY_HEADER, apiKey);

            return await _httpClient.GetFromJsonAsync<List<UserDetails>>("api/account/users") ?? new List<UserDetails>();
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _httpClient.GetFromJsonAsync<ApplicationUser>($"api/account/user/{userId}");
        }

        public async Task<ApplicationUser?> GetApplicationUserByIdAsync(string userId)
        {
            return await _httpClient.GetFromJsonAsync<ApplicationUser>($"api/account/user/{userId}");
        }

        public async Task<HttpResponseMessage> CreateUserAsync(RegisterUser user)
        {
            return await _httpClient.PostAsJsonAsync("api/account/register", user);
        }

        public async Task<HttpResponseMessage> UpdateUserAsync(ApplicationUser user)
        {
            return await _httpClient.PutAsJsonAsync($"api/account/user/{user.Id}", user);
        }

        public async Task<HttpResponseMessage> DeleteUserAsync(string id)
        {
            return await _httpClient.DeleteAsync($"api/account/user/{id}");
        }
    }
}
