using Blazored.LocalStorage;
using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Interfaces;
using ExpensesPlanner.Client.Models;
using ExpensesPlanner.Client.Services;
using ExpensesPlanner.Client.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System.Text.Json;

namespace ExpensesPlanner.Client.Pages.Account
{
    public partial class Users
    {
        [Inject] private DialogSettingsService DialogSettings { get; set; } = default!;
        [Inject] private DialogService dialogService { get; set; } = default!;
        [Inject] private IUserService userService { get; set; } = default!;
        [Inject] private ApiKeyService _apiKeyService { get; set; } = default!;
        [Inject] private ILocalStorageService _localStorage { get; set; } = default!;
        [Inject] private AuthService _authService { get; set; } = default!;

        private ApplicationUser User { get; set; } = default!;
        private List<UserDetails> users = new();
        IList<UserDetails> selectedUsers { get; set; } = new List<UserDetails>();

        protected override async Task OnInitializedAsync()
        {
            await Task.Delay(500);

            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token)) { Navigation.NavigateTo("/"); return; }

            User = await _authService.GetCurrentUserAsync(token);

            bool isValid = await _apiKeyService.ValidateApiKeyAsync(User.ApiKeyHash);

            if (!isValid)
            {
                Navigation.NavigateTo("/unathorized"); return;
            }

            await LoadAllUsersAsync();
            selectedUsers = new List<UserDetails>() { users.FirstOrDefault()! };
        }

        private async Task LoadAllUsersAsync()
        {
            users = await userService.GetAllUsers();
        }

        private async Task OpenUserDetails(string userID)
        {
            await DialogSettings.LoadStateAsync();

            await dialogService.OpenAsync<UserDetailsPopup>("Details",
                   new Dictionary<string, object>() { { "UserId", userID } },
                   new DialogOptions()
                   {
                       CssClass = "details-dialog",
                       CloseDialogOnOverlayClick = true,
                       Width = "800px",
                       /*Height = Settings != null ? Settings.Height : "712px",
                       Left = Settings != null ? Settings.Left : null,
                       Top = Settings != null ? Settings.Top : null*/
                   });

            await DialogSettings.SaveStateAsync();
        }

        private async Task OpenDeleteUserDialog(string userID)
        {
            await DialogSettings.LoadStateAsync();

            await dialogService.OpenAsync<DeleteUserPopup>("Delete User",
                   new Dictionary<string, object>() { { "UserId", userID } },
                   new DialogOptions()
                   {
                       CssClass = "userdetails-dialog",
                       CloseDialogOnOverlayClick = false,
                       /*Width = Settings != null ? Settings.Width : "712px",
                       Height = Settings != null ? Settings.Height : "712px",
                       Left = Settings != null ? Settings.Left : null,
                       Top = Settings != null ? Settings.Top : null*/
                   });

            await DialogSettings.SaveStateAsync();
            await LoadAllUsersAsync();
        }

        private async Task<string> GenerateApiKey()
        {
            Console.WriteLine(selectedUsers);
            return "yes";
            //ApiKey = await _apiKeyService.GenerateApiKeyAsync(UserId);
            //return ApiKey;
        }
    }
}
