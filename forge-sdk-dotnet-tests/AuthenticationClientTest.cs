using System;
using System.Threading.Tasks;
using Xunit;

namespace Autodesk.Forge
{
    public class AuthenticationClientTest
    {
        private string clientId;
        private string clientSecret;

        public AuthenticationClientTest()
        {
            clientId = Environment.GetEnvironmentVariable("FORGE_CLIENT_ID");
            clientSecret = Environment.GetEnvironmentVariable("FORGE_CLIENT_SECRET");
        }

        [Fact]
        public async Task TestAuthenticate()
        {
            var client = new AuthenticationClient();
            var auth = await client.Authenticate(clientId, clientSecret, "viewables:read");
            Assert.True(auth.access_token.Length > 0);
            Assert.Equal("Bearer", auth.token_type);
        }
    }
}
