using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Autodesk.Forge
{
    public class BaseClient : IDisposable
    {
        protected HttpClient Client { get; init; }

        protected IAccessTokenProvider AccessTokenProvider { get; init; }

        public BaseClient(IAccessTokenProvider accessTokenProvider, string baseAddress)
        {
            Client = new HttpClient();
            Client.BaseAddress = new Uri(baseAddress);
            AccessTokenProvider = accessTokenProvider;
        }

        protected async Task<HttpResponseMessage> GetAsync(string endpoint, IEnumerable<string> scopes)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await AccessTokenProvider.GetToken(scopes));
            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return response;
        }

        protected async Task<T> GetJsonAsync<T>(string endpoint, IEnumerable<string> scopes)
        {
            var response = await GetAsync(endpoint, scopes);
            return await response.Content.ReadFromJsonAsync<T>();
        }

        protected async Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent content, IEnumerable<string> scopes)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await AccessTokenProvider.GetToken(scopes));
            request.Content = content;
            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return response;
        }

        protected async Task<HttpResponseMessage> PutAsync(string endpoint, HttpContent content, IEnumerable<string> scopes)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await AccessTokenProvider.GetToken(scopes));
            request.Content = content;
            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return response;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Client?.Dispose();
            }
        }
    }
}
