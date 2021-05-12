using Autodesk.Forge;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Autodesk.Forge.Samples
{
    [ApiController]
    [Route("api/[controller]")]
    public class BucketsController : ControllerBase
    {
        private readonly ILogger<BucketsController> _logger;
        private readonly IForgeService _forgeService;

        public BucketsController(ILogger<BucketsController> logger, IForgeService forgeService)
        {
            _logger = logger;
            _forgeService = forgeService;
        }

        [HttpGet]
        public async Task<IEnumerable<ForgeBucket>> GetBuckets()
        {
            var buckets = await _forgeService.GetBuckets();
            return buckets;
        }

        [HttpGet("{bucketKey}/objects")]
        public async Task<IEnumerable<ForgeObject>> GetObjects(string bucketKey)
        {
            var objects = await _forgeService.GetObjects(bucketKey);
            return objects;
        }
    }
}