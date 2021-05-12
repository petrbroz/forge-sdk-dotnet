using System;
using System.Linq;
using Autodesk.Forge;

var ForgeClientID = Environment.GetEnvironmentVariable("FORGE_CLIENT_ID");
var ForgeClientSecret = Environment.GetEnvironmentVariable("FORGE_CLIENT_SECRET");

try
{
    var dataManagementClient = new DataManagementClient(new OAuthTokenProvider(ForgeClientID, ForgeClientSecret));

    var persistentBucketKeys =
        from bucket in dataManagementClient.EnumerateBuckets()
        where bucket.policyKey == "persistent"
        select bucket.bucketKey;

    await foreach (var bucketKey in persistentBucketKeys)
    {
        Console.WriteLine("Bucket: {0}", bucketKey);
        await foreach (var obj in dataManagementClient.EnumerateObjects(bucketKey))
        {
            Console.WriteLine("- {0}", obj.objectKey);
        }
    }
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
