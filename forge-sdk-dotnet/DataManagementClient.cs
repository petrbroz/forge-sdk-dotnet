using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Autodesk.Forge
{
    public class DataManagementClient : BaseClient
    {
        public record Bucket(string bucketKey, long createdDate, string policyKey);
        public record Object(string bucketKey, string objectKey, string objectId, string sha1, long size, string location);
        protected record PaginatedResponse<T>(T[] items, string next);

        protected const string DefaultBaseAddress = "https://developer.api.autodesk.com/oss/v2/";
        protected static readonly string[] ReadScopes = new string[] { "bucket:read", "data:read" };

        public DataManagementClient(IAccessTokenProvider accessTokenProvider, string baseAddress = DefaultBaseAddress)
            : base(accessTokenProvider, baseAddress)
        {}

        public async IAsyncEnumerable<Bucket> EnumerateBuckets()
        {
            var uri = "buckets";
            while (!string.IsNullOrEmpty(uri))
            {
                var response = await GetJsonAsync<PaginatedResponse<Bucket>>(uri, ReadScopes);
                foreach (var bucket in response.items)
                {
                    yield return bucket;
                }
                uri = response.next;
            }
        }

        public async Task<IEnumerable<Bucket>> ListBuckets()
        {
            var buckets = new List<Bucket>();
            var uri = "buckets";
            while (!string.IsNullOrEmpty(uri))
            {
                var response = await GetJsonAsync<PaginatedResponse<Bucket>>(uri, ReadScopes);
                buckets.AddRange(response.items);
                uri = response.next;
            }
            return buckets;
        }

        public async IAsyncEnumerable<Object> EnumerateObjects(string bucketKey)
        {
            var uri = string.Format("buckets/{0}/objects", bucketKey);
            while (!string.IsNullOrEmpty(uri))
            {
                var response = await GetJsonAsync<PaginatedResponse<Object>>(uri, ReadScopes);
                foreach (var obj in response.items)
                {
                    yield return obj;
                }
                uri = response.next;
            }
        }

        public async Task<IEnumerable<Object>> ListObjects(string bucketKey)
        {
            var objects = new List<Object>();
            var uri = string.Format("buckets/{0}/objects", bucketKey);
            while (!string.IsNullOrEmpty(uri))
            {
                var response = await GetJsonAsync<PaginatedResponse<Object>>(uri, ReadScopes);
                objects.AddRange(response.items);
                uri = response.next;
            }
            return objects;
        }
    }
}