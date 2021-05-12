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
    public class AuthController : ControllerBase
    {
        private readonly ILogger<BucketsController> _logger;
        private readonly IForgeService _forgeService;

        public AuthController(ILogger<BucketsController> logger, IForgeService forgeService)
        {
            _logger = logger;
            _forgeService = forgeService;
        }

        [HttpGet("token")]
        public async Task<ForgeToken> GetAccessToken()
        {
            var token = await _forgeService.GetAccessToken(new List<string>() { "viewables:read" });
            return token;
        }
    }
}