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

                var myusername = "frederic@programq4u.onmicrosoft.com";
                var myappId = "6c5d8069-ad47-4e99-b8a6-d8be585147fd";

                var response = await _httpClient.PostAsync($"https://ownidpfa.azurewebsites.net/api/token?username={myusername}&appId={myappId}", null);

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
