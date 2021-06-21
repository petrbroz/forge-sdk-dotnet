using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Autodesk.Forge
{
    public class DerivativesClient : BaseClient
    {
        public record BaseFormat(string type);
        public record ConvertSVFFormat(IEnumerable<string> views) : BaseFormat(ConvertType.svf.ToString());
        public record ConvertSVF2Format(IEnumerable<string> views) : BaseFormat(ConvertType.svf2.ToString());

        public record TranslationJob(string result, string urn);
        public record TranslationJobStatus(string urn, string type, string progress, string status, string region, string hasThumbnail);

        public record Object(string bucketKey, string objectKey, string objectId, string sha1, long size, string location);
        protected record PaginatedResponse<T>(T[] items, string next);

        private const string derivativesManifestEndpoint = "/derivativeservice/v2/manifest";

        private const string modelDefaultDerivativesEndpoint = "/modelderivative/v2/designdata";
        private const string modelEMEADerivativesEndpoint = "/modelderivative/v2/regions/eu/designdata";

        protected const string DefaultBaseAddress = "https://developer.api.autodesk.com";

        protected static readonly string[] DataReadWriteAll = new string[] { "data:read", "data:search", "data:create", "data:write" };
        protected static readonly string[] DataRead = new string[] { "data:read" };
        protected static readonly string[] DataWrite = new string[] { "data:create", "data:write" };

        public DerivativesClient(IAccessTokenProvider accessTokenProvider, string baseAddress = DefaultBaseAddress)
            : base(accessTokenProvider, baseAddress)
        { }

        public async Task<TranslationJob> TranslateObjectToSVFAsync(string urn, IEnumerable<ConvertSVFFormat> formats, BucketRegion region = BucketRegion.US)
        {
            var payloaObj = new
            {
                input = new { urn = urn },
                output = new
                {
                    destination = new
                    {
                        region = region.ToString().ToLower()
                    },
                    formats = formats
                }
            };

            var payload = JsonSerializer.Serialize(payloaObj);

            return await TranslateObjectAsync(payload, region);
        }

        public async Task<TranslationJob> TranslateObjectToSVF2Async(string urn, IEnumerable<ConvertSVF2Format> formats, BucketRegion region = BucketRegion.US)
        {
            var payloaObj = new
            {
                input = new { urn = urn },
                output = new
                {
                    destination = new
                    {
                        region = region.ToString().ToLower()
                    },
                    formats = formats
                }
            };

            var payload = JsonSerializer.Serialize(payloaObj);

            return await TranslateObjectAsync(payload, region);
        }

        private async Task<TranslationJob> TranslateObjectAsync(string payload, BucketRegion region)
        {
            using var stringContent = new StringContent(payload, Encoding.UTF8, "application/json");

            var uri = region == BucketRegion.US ? $"{modelDefaultDerivativesEndpoint}/job" : $"{modelEMEADerivativesEndpoint}/job";
            var response = await PostAsync(uri, stringContent, DataReadWriteAll);

            var returnValue = await response.Content.ReadFromJsonAsync<TranslationJob>();

            return returnValue;
        }

        public async Task<TranslationJobStatus> CheckTranslationStatus(string urn, BucketRegion region)
        {
            var uri = region == BucketRegion.US ? modelDefaultDerivativesEndpoint : modelEMEADerivativesEndpoint;
            uri = $"{uri}/{urn}/manifest";

            var response = await GetAsync(uri, DataRead);

            var result = await response.Content.ReadFromJsonAsync<TranslationJobStatus>();

            return result;
        }

        public async Task<byte[]> GetModelAsync(string urn)
        {
            var uri = $"{derivativesManifestEndpoint}/{urn}";
            var response = await GetAsync(uri, DataRead);

            var result = await response.Content.ReadAsByteArrayAsync();

            return result;
        }
    }
}
