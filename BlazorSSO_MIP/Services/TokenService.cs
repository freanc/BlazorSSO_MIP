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
                // var myusername = "freanc";
                //var myappId = "app1";

                //var myusername = "frederic@programq4u.onmicrosoft.com";
                //var myappId = "76f38104-dd57-4eea-bc05-1251bb7edf60";

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
