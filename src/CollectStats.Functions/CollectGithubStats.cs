using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CollectStats_Functions;
using Microsoft.Extensions.Options;
using CollectStats_Functions.Services;

namespace CollectStats.Functions
{
    public class CollectGithubStats
    {
        private readonly AppOptions _appOptions;
        private readonly ILogger<CollectGithubStats> _logger;
        private readonly IGetFromGithub _github;

        public CollectGithubStats(IOptions<AppOptions> options, ILogger<CollectGithubStats> logger, IGetFromGithub github)
        {
            _appOptions = options.Value;
            _logger = logger;
            _github = github;
        }

        [FunctionName("CollectGithubStats")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var stats = await _github.GetStats("hmdev-test");
            

            return new OkObjectResult("");
        }
    }
}
