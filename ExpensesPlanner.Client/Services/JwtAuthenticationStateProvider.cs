using Blazored.LocalStorage;
using ExpensesPlanner.Client.Pages.Account;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ExpensesPlanner.Client.Services
{
    public class JwtAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IJSRuntime _jsRunTime;
        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();

        public JwtAuthenticationStateProvider(ILocalStorageService localStorage, IJSRuntime jSRuntime)
        {
            _localStorage = localStorage;
            _jsRunTime = jSRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {

            var savedToken = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(savedToken))
                return GetAnonymousState();

            try
            {
                var token = _tokenHandler.ReadJwtToken(savedToken);

                // Verify if the token is expired
                if (token.ValidTo < DateTime.UtcNow)
                {
                    // Invalid token - Remove it and return unauthenticated user state
                    await _localStorage.RemoveItemAsync("authToken");
                    return GetAnonymousState();
                }


                var identity = GetClaimsIdentity(savedToken);
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }


            catch
            {
                // Invalid token - Remove it and return unauthenticated user state
                await _localStorage.RemoveItemAsync("authToken");
                return GetAnonymousState();
            }

        }

        public async Task MarkUserAsAuthenticatedAsync(string token)
        {
            await _localStorage.SetItemAsync("authToken", token);

            var identity = GetClaimsIdentity(token);
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task MarkUserAsLoggedOutAsync()
        {
            await _localStorage.RemoveItemAsync("authToken");
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        private AuthenticationState GetAnonymousState()
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        private ClaimsIdentity GetClaimsIdentity(string token)
        {            
            var jwtToken = _tokenHandler.ReadJwtToken(token);
            var claims = jwtToken.Claims;
            return new ClaimsIdentity(claims, "jwt");
        }
    }
}
