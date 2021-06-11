using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
        protected static readonly string[] BucketReadWriteAllScopes = new string[] { "bucket:read", "bucket:create", "bucket:update", "bucket:delete" };
        protected static readonly string[] DataReadWriteAllScopes = new string[] { "data:read", "data:search", "data:create", "data:write" };

        public DataManagementClient(IAccessTokenProvider accessTokenProvider, string baseAddress = DefaultBaseAddress)
            : base(accessTokenProvider, baseAddress)
        { }

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

        public async Task<Object> GetObjectDetailsAsync(string bucketKey, string objectKey)
        {
            Object returnValue = null;
            var uri = $"buckets/{bucketKey}/objects/{objectKey}/details";
            returnValue = await GetJsonAsync<Object>(uri, ReadScopes);

            return returnValue;
        }

        public async Task<Object> UploadObjectAsync(string bucketKey, string objectKey, Stream stream)
        {
            using HttpContent httpContent = new StreamContent(stream);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/stream");

            var uri = $"buckets/{bucketKey}/objects/{objectKey}";

            var response = await PutAsync(uri, httpContent, DataReadWriteAllScopes);
            response.EnsureSuccessStatusCode();

            var rObject = await response.Content.ReadFromJsonAsync<Object>();

            return rObject;
        }

        public async Task<Object> UploadObjectAsync(string bucketKey, string objectKey, Stream stream, int chunkSizeInMB = 5)
        {
            Object returnValue = null;

            var maxUploadSizeInMB = 100 * 1024 * 1024;

            long fileSize = stream.Length;

            if (fileSize > maxUploadSizeInMB) // object is greater then 100 mb => must be uploaded in chunks
            {
                var processId = $"-{RandomString(9)}";
                long chunkSize = chunkSizeInMB * 1024 * 1024;
                byte[] buffer = new byte[chunkSize];

                var remainingSize = fileSize;
                var fileSizeMB = fileSize / 1048576;

                double blup = (double)fileSize / (double)chunkSize;
                double nbChunks = Math.Ceiling(blup);

                long start;
                long end;

                for (var i = 0; i < nbChunks; i++)
                {
                    using var ms = new MemoryStream();
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read > 0)
                    {
                        ms.Write(buffer, 0, read);
                        ms.Position = 0;

                        start = i * chunkSize;
                        end = Math.Min(fileSize, (i + 1) * chunkSize) - 1;

                        returnValue = await UploadObjectInChunksAsync(bucketKey, objectKey, ms, read, start, end, fileSize, processId);

                        remainingSize = remainingSize - chunkSize;

                        if (remainingSize > 0 && remainingSize < chunkSize)
                        {
                            buffer = new byte[remainingSize];
                        }
                    }
                }
            }
            else
            {
                returnValue = await UploadObjectAsync(bucketKey, objectKey, stream);
            }

            return returnValue;
        }

        public async Task<Object> UploadObjectInChunksAsync(string bucketKey, string objectKey, Stream chunk, long length, long start, long end, long fileSize, string processId)
        {
            var range = $"bytes {start}-{end}/{fileSize}"; // for logging

            using HttpContent httpContent = new StreamContent(chunk);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/stream");
            httpContent.Headers.ContentLength = length;
            httpContent.Headers.ContentRange = new ContentRangeHeaderValue(start, end, fileSize);
            httpContent.Headers.Add("Session-Id", processId);

            var uri = $"buckets/{bucketKey}/objects/{objectKey}/resumable";

            var response = await PutAsync(uri, httpContent, DataReadWriteAllScopes);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {

                var rObject = await response.Content.ReadFromJsonAsync<Object>();

                return rObject;
            }

            return null;
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}