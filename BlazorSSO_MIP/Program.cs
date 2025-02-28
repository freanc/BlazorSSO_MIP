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




// Register the AuthenticationStateProviderService
builder.Services.AddScoped<AuthenticationStateProviderService>();

var host = builder.Build();

// Ensure AuthenticationStateProviderService is instantiated
var authStateProviderService = host.Services.GetRequiredService<AuthenticationStateProviderService>();

await host.RunAsync();