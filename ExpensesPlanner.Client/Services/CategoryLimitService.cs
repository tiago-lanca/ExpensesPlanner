using ExpensesPlanner.Client.Models;
using System.Net.Http.Json;

namespace ExpensesPlanner.Client.Services
{
    public class CategoryLimitService
    {
        private readonly HttpClient _httpClient;
        public CategoryLimitService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<UserCategoryLimit> GetUserCategoryLimitsAsync(string userId)
        {
            return await _httpClient.GetFromJsonAsync<UserCategoryLimit>($"api/categorylimit/{userId}") ?? new UserCategoryLimit();
        }
        public async Task<HttpResponseMessage> UpdateUserCategoryLimitsAsync(UserCategoryLimit userCategoryLimit)
        {
            return await _httpClient.PutAsJsonAsync($"api/categorylimit/{userCategoryLimit.UserId}", userCategoryLimit);
        }
    }
}
