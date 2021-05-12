using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Autodesk.Forge
{
    public interface IAccessTokenProvider
    {
        Task<string> GetToken(IEnumerable<string> scopes);
    }

    public class SimpleTokenProvider : IAccessTokenProvider
    {
        protected string AccessToken { get; init; }

        public SimpleTokenProvider(string accessToken)
        {
            AccessToken = accessToken;
        }

        public Task<string> GetToken(IEnumerable<string> scopes)
        {
            return Task.FromResult<string>(AccessToken);
        }
    }

    public class OAuthTokenProvider : IAccessTokenProvider
    {
        protected record TokenCache(string access_token, DateTime expires);

        public string ClientID { get; init; }
        public string ClientSecret { get; init; }
        protected Dictionary<string, TokenCache> cache = new ();

        public OAuthTokenProvider(string clientId, string clientSecret)
        {
            (ClientID, ClientSecret) = (clientId, clientSecret);
        }

        public async Task<string> GetToken(IEnumerable<string> scopes)
        {
            var cacheKey = string.Join('+', scopes);
            if (cache.ContainsKey(cacheKey))
            {
                var tokenCache = cache[cacheKey];
                if (tokenCache.expires > DateTime.Now)
                {
                    return tokenCache.access_token;
                }
                else
                {
                    cache.Remove(cacheKey);
                }
            }
            var client = new AuthenticationClient();
            var auth = await client.Authenticate(ClientID, ClientSecret, scopes);
            cache.Add(cacheKey, new TokenCache(auth.access_token, DateTime.Now.AddSeconds(auth.expires_in)));
            return auth.access_token;
        }
    }
}
