using Blazored.LocalStorage;
using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Models;
using ExpensesPlanner.Client.RootPages;
using ExpensesPlanner.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using Radzen.Blazor;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ExpensesPlanner.Client.Pages.Account
{
    public partial class LoginPopup
    {
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private HttpClient HttpClient { get; set; } = default!;
        [Inject] private AuthService AuthService { get; set; } = default!;
        [Inject] private NotificationService NotificationService { get; set; } = default!;
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        private async Task OnLogin(LoginArgs args)
        {
            try
            {
                var loginDto = new LoginDto
                {
                    Email = args.Username,
                    Password = args.Password
                };
                

                var response = await AuthService.LoginAsync(loginDto);

                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<TokenResponse>();

                    await LocalStorage.SetItemAsync("authToken", user.Token);
                    Console.WriteLine($"User token: {user.Token}");
                    ((JwtAuthenticationStateProvider)AuthenticationStateProvider).NotifyUserAuthentication(user.Token);

                    HttpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", user.Token);

                    Navigation.NavigateTo(PagesRoutes.AllExpenses);
                    var currentUser = await AuthService.GetCurrentUserAsync();
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Username or Password incorrect.",
                        Duration = 4000,
                        ShowProgress = true,
                        CloseOnClick = true,
                        Payload = DateTime.Now
                    });
                }
            }

            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"Error: {ex.Message}",
                    Duration = 4000,
                    ShowProgress = true,
                    CloseOnClick = true,
                    Payload = DateTime.Now
                });
            }
        }

        private async Task GoBackLogin()
        {
            Navigation.NavigateTo("/");
        }
    }
}
