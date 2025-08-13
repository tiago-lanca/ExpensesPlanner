using Blazored.LocalStorage;
using ExpensesPlanner.Client.Pages.Account;
using ExpensesPlanner.Client.RootPages;
using ExpensesPlanner.Client.Services;
using ExpensesPlanner.Client.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using MongoDB.Driver;
using Radzen;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ExpensesPlanner.Components.Layout
{
    public partial class NavMenu
    {
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private DialogSettingsService DialogSettings { get; set; } = default!;
        [Inject] private DialogService DialogService { get; set; } = default!;
        [Inject] private ILocalStorageService _localStorage { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private NotificationService NotificationService { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
        [Inject] private HttpClient HttpClient { get; set; } = default!;

        private string? token = string.Empty;
        private string? profilePictureUrl = string.Empty;
        private string? currentUrl;

        protected override async Task OnInitializedAsync()
        {
            ((JwtAuthenticationStateProvider)AuthStateProvider).OnAuthenticationStateChangedProfilePicture += async () =>
            {
                profilePictureUrl = await LoadUserPhotoAsync();
                StateHasChanged();
            };

            token = await _localStorage.GetItemAsync<string>("authToken");

            profilePictureUrl = await LoadUserPhotoAsync();
        }

        public async Task OpenLoginPopup()
        {
            await DialogSettings.LoadStateAsync();

            await DialogService.OpenAsync<LoginPopup>("",
                    new Dictionary<string, object>(),
                    new DialogOptions()
                    {
                        //CssClass = "login-dialog",
                        CloseDialogOnOverlayClick = false,
                        Width = "0",
                        //Height = Settings != null ? Settings.Height : "712px",
                        //Left = Settings != null ? Settings.Left : null,
                        //Top = Settings != null ? Settings.Top : null
                    });

            await DialogSettings.SaveStateAsync();
            StateHasChanged();
        }

        public async Task Logout()
        {
            await ((JwtAuthenticationStateProvider)AuthStateProvider).MarkUserAsLoggedOutAsync();
            Navigation.NavigateTo("/");

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Info",
                Detail = "You have logged out successfully.",
                Duration = 4000,
                ShowProgress = true,
                CloseOnClick = true,
                Payload = DateTime.Now
            });
        }

        private async Task<string> LoadUserPhotoAsync()
        {
            token = await _localStorage.GetItemAsync<string>("authToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "api/account/user/photo");

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await HttpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var photo = await response.Content.ReadAsByteArrayAsync();
                    var base64Image = Convert.ToBase64String(photo);
                    return $"data:image/png;base64,{base64Image}";
                }
                else
                {
                    // Handle error, e.g., show notification or log
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to fetch user photo. Status: {response.StatusCode}, Error: {errorMessage}");
                    return "Failed";
                }
            }

            return "Failed";
        }

        public async Task GoToExpenses()
        {
            token = await _localStorage.GetItemAsync<string>("authToken");

            if (token is null)
            {
                Navigation.NavigateTo("/");

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Warning",
                    Detail = "Access denied. Log into an account.",
                    Duration = 4000,
                    ShowProgress = true,
                    CloseOnClick = true,
                    Payload = DateTime.Now
                });
                return;
            }

            StateHasChanged();
            Navigation.NavigateTo(PagesRoutes.AllExpenses);
            
            return;
        }

        public async Task GoToAllUsers()
        {
            token = await _localStorage.GetItemAsync<string>("authToken");

            if (token is null)
            {
                Navigation.NavigateTo("/");
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Warning",
                    Detail = "Access denied. Log into an account.",
                    Duration = 4000,
                    ShowProgress = true,
                    CloseOnClick = true,
                    Payload = DateTime.Now
                });
                return;
            }

            StateHasChanged();
            Navigation.NavigateTo(PagesRoutes.AllUsers);

            return;
        }

        public async Task GoToMyFinances()
        {
            token = await _localStorage.GetItemAsync<string>("authToken");

            if (token is null)
            {
                Navigation.NavigateTo("/");

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Warning",
                    Detail = "Access denied. Log into an account.",
                    Duration = 4000,
                    ShowProgress = true,
                    CloseOnClick = true,
                    Payload = DateTime.Now
                });
                return;
            }

            StateHasChanged();
            Navigation.NavigateTo(PagesRoutes.MyFinances);

            return;
        }

        public string GetButtonClass(string url)
        {
            var currentUrl = Navigation.Uri.Split('?')[0]; // Remove query parameters for comparison
            return currentUrl.EndsWith(url, StringComparison.OrdinalIgnoreCase) ? "nav-link navmenu-button active" : "nav-link";            
        }

        private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            currentUrl = e.Location;
            StateHasChanged();
        }

        public void Dispose()
        {
            Navigation.LocationChanged -= HandleLocationChanged;
        }
    }
}
