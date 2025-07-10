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
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> LoginAsync(LoginDto dto)
        {
            return await _httpClient.PostAsJsonAsync("api/auth/login", dto);
        }

        public async Task<ApplicationUser> GetCurrentUserAsync(string token)
        {
            // Adds the token to the Authorization header for the request
            _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("api/auth/me");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApplicationUser>();
            }
            return null!;
        }

        public async Task<bool> IsUserLoggedInAsync()
        {
            var token = await LocalStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token)) return false;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            if(jwt.ValidTo < DateTime.UtcNow)
            {
                await LocalStorage.RemoveItemAsync("authToken");
                return false;
            }

            return true;
        }
    }
}
