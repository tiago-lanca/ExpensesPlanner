using System.Net.Http.Json;

namespace ExpensesPlanner.Client.Services
{
    public class ApiKeyService
    {
        private readonly HttpClient _httpClient;
        public const string API_KEY_HEADER = "x-api-key";

        public ApiKeyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateApiKeyAsync(string userId)
        {
            return await _httpClient.GetStringAsync($"api/apikey/generate/{userId}");
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey)
        {
            _httpClient.DefaultRequestHeaders.Remove(API_KEY_HEADER);
            _httpClient.DefaultRequestHeaders.Add(API_KEY_HEADER, apiKey);
                        

            var result = await _httpClient.GetAsync($"api/apikey/validate");

            return result.IsSuccessStatusCode ? true : false;
        }
    }
}
