using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Services.Implementations
{
    public class OAuthService : Service, IOAuthService
    {
        public OAuthService(IConfiguration configuration) 
            : base(configuration.GetSection("Addresses")["Auth"]) { }

        public async Task<Tuple<string,string>> GetAccessAndRefreshTokens(string username, string password)
        {
            var response = await PostForm("connect/token", new Dictionary<string, string>
            {
                {"grant_type", "client_credentials" },
                {"client_id", "Gateway" },
                {"client_secret", "secret" },
                {"scope", "offline_access" },
                {"username", username },
                {"password", password }
            });
            if (response.IsSuccessStatusCode)
            {
                var oauthResponse = JsonConvert.DeserializeObject<OAuthResponse>(response.Content.ReadAsStringAsync().Result);
                return new Tuple<string, string>(oauthResponse.AccessToken, oauthResponse.RefreshToken);
            }
            return null;
        }

        public async Task<Tuple<string, string>> GetAccessAndRefreshTokens(string refreshToken)
        {
            var response = await PostForm("connect/token", new Dictionary<string, string>
            {
                {"grant_type", "password" },
                {"client_id", "Gateway" },
                {"client_secret", "secret" },
                {"refresh_token", refreshToken },
            });
            if (response.IsSuccessStatusCode)
            {
                var oauthResponse = JsonConvert.DeserializeObject<OAuthResponse>(response.Content.ReadAsStringAsync().Result);
                return new Tuple<string, string>(oauthResponse.AccessToken, oauthResponse.RefreshToken);
            }
            return null;
        }
    }
    class OAuthResponse
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }
        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshToken { get; set; }
    }
}
