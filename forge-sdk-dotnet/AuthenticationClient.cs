using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Autodesk.Forge
{
    public class AuthenticationClient
    {
        public record TwoLeggedOAuth(string token_type, string access_token, int expires_in);
        public record ThreeLeggedOAuth(string token_type, string access_token, string refresh_token, int expires_in);

        protected const string DefaultBaseAddress = "https://developer.api.autodesk.com/authentication/v1/";

        protected HttpClient Client { get; init; }

        public AuthenticationClient(string baseAddress = DefaultBaseAddress)
        {
            Client = new HttpClient();
            Client.BaseAddress = new Uri(baseAddress);
        }

        public async Task<TwoLeggedOAuth> Authenticate(string clientId, string clientSecret, params string[] scopes)
        {
            return await Authenticate(clientId, clientSecret, (IEnumerable<string>)scopes);
        }

        public async Task<TwoLeggedOAuth> Authenticate(string clientId, string clientSecret, IEnumerable<string> scopes)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentException("Missing argument: clientId");
            }
            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new ArgumentException("Missing argument: clientSecret");
            }
            var parameters = new List<KeyValuePair<string, string>>
            {
                new("client_id", clientId),
                new("client_secret", clientSecret),
                new("grant_type", "client_credentials"),
                new("scope", string.Join(" ", scopes))
            };
            var response = await Client.PostAsync("authenticate", new FormUrlEncodedContent(parameters));
            // if (!response.IsSuccessStatusCode)
            // {
            //     System.Console.WriteLine(response.RequestMessage.ToString());
            //     var error = await response.Content.ReadAsStringAsync();
            //     throw new HttpRequestException(error);
            // }
            response.EnsureSuccessStatusCode();
            var authentication = await response.Content.ReadFromJsonAsync<TwoLeggedOAuth>();
            if (authentication == null)
            {
                throw new HttpRequestException("Could not retrieve authentication data.");
            }
            return authentication;
        }

        public async Task<ThreeLeggedOAuth> GetToken(string clientId, string clientSecret, string code, string redirectUri)
        {
            var parameters = new List<KeyValuePair<string, string>>
            {
                new("client_id", clientId),
                new("client_secret", clientSecret),
                new("grant_type", "authorization_code"),
                new("code", code),
                new("redirect_uri", redirectUri)
            };
            var response = await Client.PostAsync("gettoken", new FormUrlEncodedContent(parameters));
            // if (!response.IsSuccessStatusCode)
            // {
            //     System.Console.WriteLine(response.RequestMessage.ToString());
            //     var error = await response.Content.ReadAsStringAsync();
            //     throw new HttpRequestException(error);
            // }
            response.EnsureSuccessStatusCode();
            var authentication = await response.Content.ReadFromJsonAsync<ThreeLeggedOAuth>();
            if (authentication == null)
            {
                throw new HttpRequestException("Could not retrieve authentication data.");
            }
            return authentication;
        }

        public async Task<ThreeLeggedOAuth> RefreshToken(string clientId, string clientSecret, string refreshToken, IEnumerable<string> scopes)
        {
            var parameters = new List<KeyValuePair<string, string>>
            {
                new("client_id", clientId),
                new("client_secret", clientSecret),
                new("grant_type", "refresh_token"),
                new("refresh_token", refreshToken),
                new("scope", string.Join(" ", scopes))
            };
            var response = await Client.PostAsync("refreshtoken", new FormUrlEncodedContent(parameters));
            // if (!response.IsSuccessStatusCode)
            // {
            //     System.Console.WriteLine(response.RequestMessage.ToString());
            //     var error = await response.Content.ReadAsStringAsync();
            //     throw new HttpRequestException(error);
            // }
            response.EnsureSuccessStatusCode();
            var authentication = await response.Content.ReadFromJsonAsync<ThreeLeggedOAuth>();
            if (authentication == null)
            {
                throw new HttpRequestException("Could not retrieve authentication data.");
            }
            return authentication;
        }
    }
}