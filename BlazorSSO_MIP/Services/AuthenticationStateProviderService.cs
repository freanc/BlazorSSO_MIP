using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Threading.Tasks;

namespace BlazorSSO_MIP.Services
{
    public class AuthenticationStateProviderService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IAccessTokenProvider _provider;
        private readonly TokenService _tokenService;

        public AuthenticationStateProviderService(AuthenticationStateProvider authenticationStateProvider, IAccessTokenProvider provider, TokenService tokenService)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _provider = provider;
            _tokenService = tokenService;
            _authenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        }

        private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            var authState = task.Result;

            if (authState.User.Identity.IsAuthenticated)
            {
                // User is authenticated, retrieve the custom token
                RetrieveCustomToken();
            }
        }

        private async void RetrieveCustomToken()
        {
            var result = await _provider.RequestAccessToken();
            if (result.TryGetToken(out var token))
            {
                var username = token.Value; // Extract username from the token if needed
                var appName = "YourAppName";

                // Call custom IDP to get the token
                var customToken = await _tokenService.GetTokenAsync(username, appName);

                // Store or use the custom token as needed
                Console.WriteLine($"Retrieved custom token: {customToken}");
            }
        }
    }
}