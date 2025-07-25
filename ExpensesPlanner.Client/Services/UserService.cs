﻿using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Interfaces;
using System.Net.Http.Json;

namespace ExpensesPlanner.Client.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<UserDetails>> GetAllUsers()
        {
            return await _httpClient.GetFromJsonAsync<List<UserDetails>>("api/account/users") ?? new List<UserDetails>();
        }

        public async Task<UserDetails> GetUserByIdAsync(string userId)
        {
            return await _httpClient.GetFromJsonAsync<UserDetails>($"api/account/user/{userId}");
        }

        public async Task<HttpResponseMessage> CreateUserAsync(RegisterUser user)
        {
            return await _httpClient.PostAsJsonAsync("api/account/register", user);
        }

        public async Task<HttpResponseMessage> UpdateUserAsync(UserDetails user)
        {
            return await _httpClient.PutAsJsonAsync($"api/account/user/{user.Id}", user);
        }

        public async Task<HttpResponseMessage> DeleteUserAsync(string id)
        {
            return await _httpClient.DeleteAsync($"api/account/user/{id}");
        }
    }
}
