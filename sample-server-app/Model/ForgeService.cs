using Autodesk.Forge;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Autodesk.Forge.Samples
{
    public record ForgeBucket(string name);

    public record ForgeObject(string name, string urn);

    public record ForgeToken(string access_token, int expires_in);

    public interface IForgeService
    {
        Task<IEnumerable<ForgeBucket>> GetBuckets();
        Task<IEnumerable<ForgeObject>> GetObjects(string bucketName);
        Task<ForgeToken> GetAccessToken(IEnumerable<string> scopes);
    }

    public class ForgeService : IForgeService
    {
        private readonly OAuthTokenProvider _tokenProvider;
        private readonly DataManagementClient _dataManagementClient;

        public ForgeService(string clientId, string clientSecret)
        {
            _tokenProvider = new OAuthTokenProvider(clientId, clientSecret);
            _dataManagementClient = new DataManagementClient(_tokenProvider);
        }

        public async Task<IEnumerable<ForgeBucket>> GetBuckets()
        {
            var buckets = await _dataManagementClient.ListBuckets();
            return buckets.Select(bucket => new ForgeBucket(bucket.bucketKey));
        }

        public async Task<IEnumerable<ForgeObject>> GetObjects(string bucketName)
        {
            var objects = await _dataManagementClient.ListObjects(bucketName);
            return objects.Select(obj => new ForgeObject(obj.objectKey, Base64Encode(obj.objectId)));
        }

        public async Task<ForgeToken> GetAccessToken(IEnumerable<string> scopes)
        {
            var access_token = await _tokenProvider.GetToken(scopes);
            return new ForgeToken(access_token, 3600);
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes).TrimEnd('=');
        }
    }
}
