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
        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
        private readonly IJSRuntime _jsRunTime;

        public JwtAuthenticationStateProvider(ILocalStorageService localStorage, IJSRuntime jSRuntime)
        {
            _localStorage = localStorage;
            _jsRunTime = jSRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_jsRunTime is IJSInProcessRuntime)
            {

                var savedToken = await _localStorage.GetItemAsync<string>("authToken");

                if (string.IsNullOrWhiteSpace(savedToken))
                    return GetAnonymousState();

                try
                {
                    var token = _tokenHandler.ReadJwtToken(savedToken);

                    if (token.ValidTo < DateTime.UtcNow)
                    {
                        // Invalid token - Remove it and return unauthenticated user state
                        await _localStorage.RemoveItemAsync("authToken");
                        return GetAnonymousState();
                    }

                    var claims = GetClaims(token);
                    var identity = new ClaimsIdentity(claims, "jwt");
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
            else
            {
                return GetAnonymousState();
            }
        }

        public async Task MarkUserAsAuthenticated(string token)
        {
            await _localStorage.SetItemAsync("authToken", token);

            //var authenticatedUser = _tokenHandler.ReadJwtToken(token);
            //var claims = GetClaims(authenticatedUser);

            //var nameClaimType = claims.FirstOrDefault(c => c.Type == "name" || c.Type == "unique_name")?.Type ?? ClaimTypes.Name;
            
            var identity = new ClaimsIdentity(claims, "jwt", nameClaimType, ClaimTypes.Role);
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void NotifyUserLogout()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
        }

        private AuthenticationState GetAnonymousState()
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        private IEnumerable<Claim> GetClaims(JwtSecurityToken token)
        {
            return token.Claims;
        }
    }
}
