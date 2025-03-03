# Single Sign-On (SSO) Proof of Concept

This Proof of Concept (POC) demonstrates a basic implementation of Single Sign-On (SSO) and token retrieval from a custom-developed identity provider (IDP) using .NET, deployed in Azure.

## Overview

1. **SSO Implementation**: SSO is implemented using the client ID of the app registration.
2. **Handler Setup**: A handler is configured to execute after the authentication process.
3. **Token Retrieval**: The IDP is called within the handler using the username (claim: `unique_name`) and the client ID of the app registration (claim: `appid`).
4. **Claims Storage**: The claims are stored in memory using a scoped claims service.

## Prerequisites

- .NET SDK
- Azure Subscription
- App Registration in Azure Active Directory (AAD)
- Custom-developed Identity Provider (IDP)

## Setup

### 1. App Registration in Azure AD

Register your application in Azure Active Directory and obtain the client ID and tenant ID.

### 2. Configure the .NET Application

Add the necessary NuGet packages for authentication and claims handling.

### 3. Implement SSO

Configure the authentication middleware and services in your `Program.cs` file:

```csharp name=Program.cs
using BlazorSSO_MIP;
using BlazorSSO_MIP.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HTTP Client
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register the IHttpClientFactory service
builder.Services.AddHttpClient();

// Configure MSAL Authentication for SSO
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add("openid");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("profile");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("email");
});

// Custom IDP Token Service
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ClaimsService>();
builder.Services.AddScoped<AuthenticationStateProviderService>();

var host = builder.Build();

// Ensure AuthenticationStateProviderService is instantiated
var authStateProviderService = host.Services.GetRequiredService<AuthenticationStateProviderService>();

await host.RunAsync();
```

### 4. Create a Claims Service

Create a scoped claims service to store claims in memory:

```csharp name=Services/ClaimsService.cs
using System.Security.Claims;

namespace BlazorSSO_MIP.Services
{
    public class ClaimsService
    {
        private readonly List<Claim> _claims = new List<Claim>();

        public void SetClaims(IEnumerable<Claim> claims)
        {
            _claims.Clear();
            _claims.AddRange(claims);
        }

        public IEnumerable<Claim> GetClaims()
        {
            return _claims;
        }
    }
}
```

### 5. Implement the Authentication State Provider Service

Create a service to handle authentication state changes and retrieve custom tokens:

```csharp name=Services/AuthenticationStateProviderService.cs
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
            RetrieveCustomToken(authState.User);
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
```

### 6. Implement the Token Service

Create a service to call the custom IDP and retrieve tokens:

```csharp name=Services/TokenService.cs
using System.Net.Http.Json;

namespace BlazorSSO_MIP.Services
{
    public class TokenService
    {
        private readonly HttpClient _httpClient;

        public TokenService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetTokenAsync(string username, string appId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"https://ownidpfa.azurewebsites.net/api/token?username={username}&appId={appId}", null);

                response.EnsureSuccessStatusCode();

                var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                return tokenResponse?.Token;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class TokenResponse
    {
        public string Token { get; set; }
    }
}
```

## Running the Application

1. Build and run the application.
2. Authenticate using your Azure AD credentials.
3. The handler will be executed, and the token will be retrieved and stored in memory.

## Conclusion

This POC provides a basic implementation of SSO and token retrieval from a custom-developed IDP in .NET deployed in Azure. You can extend this implementation based on your specific requirements.
