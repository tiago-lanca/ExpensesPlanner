using Blazored.LocalStorage;
using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Models;
using Microsoft.AspNetCore.Components;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ExpensesPlanner.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private ILocalStorageService _localStorage { get; set; } = default!;
        private string? ApiKey = string.Empty;

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task<HttpResponseMessage> LoginAsync(LoginDto dto)
        {
            return await _httpClient.PostAsJsonAsync("api/auth/login", dto);
        }

        public async Task<ApplicationUser> GetCurrentUserAsync(string token)
        {
            ApiKey = await _localStorage.GetItemAsync<string>("apiKeyHash");

            // Adds the token to the Authorization header for the request
            _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

            _httpClient.DefaultRequestHeaders.Remove(ApiKeyService.API_KEY_HEADER);
            _httpClient.DefaultRequestHeaders.Add(ApiKeyService.API_KEY_HEADER, ApiKey);

            var response = await _httpClient.GetAsync("api/auth/me");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApplicationUser>() ?? null!;
            }
            return null!;
        }

        public async Task<bool> IsUserLoggedInAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token)) return false;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            if(jwt.ValidTo < DateTime.UtcNow)
            {
                await _localStorage.RemoveItemAsync("authToken");
                return false;
            }

            return true;
        }
    }
}
