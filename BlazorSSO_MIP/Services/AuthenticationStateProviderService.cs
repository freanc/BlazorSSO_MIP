using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BlazorSSO_MIP.Services
{
    public class AuthenticationStateProviderService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IAccessTokenProvider _provider;
        private readonly TokenService _tokenService;
        private readonly ClaimsService _claimsService;
        private readonly ILogger<AuthenticationStateProviderService> _logger;

        public AuthenticationStateProviderService(AuthenticationStateProvider authenticationStateProvider,
                                                  IAccessTokenProvider provider,
                                                  TokenService tokenService,
                                                  ClaimsService claimsService,
                                                  ILogger<AuthenticationStateProviderService> logger)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _provider = provider;
            _tokenService = tokenService;
            _claimsService = claimsService;
            _logger = logger;
            _logger.LogInformation("AuthenticationStateProviderService initialized.");
            _authenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        }

        private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            var authState = task.Result;
            _logger.LogInformation("Authentication state changed.");

            if (authState.User.Identity.IsAuthenticated)
            {
                // User is authenticated, retrieve the custom token
                _logger.LogInformation("User is authenticated. Retrieving custom token.");
                RetrieveCustomToken(authState.User);
            }
        }

        private async void RetrieveCustomToken(ClaimsPrincipal user)
        {
            var result = await _provider.RequestAccessToken();
            if (result.TryGetToken(out var token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token.Value) as JwtSecurityToken;

                if (jwtToken != null)
                {
                    var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;
                    var appName = jwtToken.Claims.FirstOrDefault(c => c.Type == "appid")?.Value;

                    // Call custom IDP to get the token
                    var customToken = await _tokenService.GetTokenAsync(username, appName);

                    // Parse the custom token to extract claims
                    var customJwtToken = handler.ReadToken(customToken) as JwtSecurityToken;
                    if (customJwtToken != null)
                    {
                        var claims = customJwtToken.Claims;

                        // Store claims in the ClaimsService
                        _claimsService.SetClaims(claims);

                        _logger.LogInformation($"Retrieved and stored custom token claims: {string.Join(", ", claims.Select(c => $"{c.Type}: {c.Value}"))}");
                    }
                }
            }
        }
    }
}